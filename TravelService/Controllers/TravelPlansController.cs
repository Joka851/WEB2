using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TravelService.Data;
using TravelService.DTOs;
using TravelService.Models;

namespace TravelService.Controllers
{
    [ApiController]
    [Route("api/travel-plans")]
    [Authorize]
    public class TravelPlansController : ControllerBase
    {
        private readonly TravelDbContext _context;
        private readonly ILogger<TravelPlansController> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public TravelPlansController(
            TravelDbContext context,
            ILogger<TravelPlansController> logger,
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration)
        {
            _context = context;
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            return int.Parse(userIdClaim?.Value ?? "0");
        }

        private string GetCurrentUserRole()
        {
            return User.FindFirst(ClaimTypes.Role)?.Value ?? "User";
        }

       
        private async Task<bool> HasValidEditShareToken(int travelPlanId)
        {
            if (!Request.Headers.TryGetValue("X-Share-Token", out var tokenValue))
                return false;

            var token = tokenValue.ToString();
            if (string.IsNullOrWhiteSpace(token))
                return false;

            var shareToken = await _context.ShareTokens.FirstOrDefaultAsync(s =>
                s.Token == token &&
                s.TravelPlanId == travelPlanId &&
                s.AccessType == ShareToken.ACCESS_TYPE_EDIT &&
                s.IsActive &&
                !s.IsDeleted);

            return shareToken != null && !shareToken.IsExpired();
        }

       
        private async Task DeleteExpensesInFinanceService(int travelPlanId)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("FinanceService");
                var internalKey = _configuration["Internal:ApiKey"];
                var request = new HttpRequestMessage(HttpMethod.Delete, $"/api/internal/travel-plans/{travelPlanId}/expenses");
                request.Headers.Add("X-Internal-Api-Key", internalKey);

                var response = await client.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning(
                        "FinanceService je vratio {StatusCode} pri cascade brisanju troškova za plan {PlanId}",
                        response.StatusCode, travelPlanId);
                }
                else
                {
                    _logger.LogInformation("Cascade: FinanceService obrisao troškove za plan {PlanId}", travelPlanId);
                }
            }
            catch (Exception ex)
            {
                // Namerno ne bacamo dalje - FinanceService možda trenutno nije dostupan,
                // ali brisanje putnog plana ne sme da propadne zbog toga.
                _logger.LogError(ex, "Greška pri pozivanju FinanceService za cascade brisanje troškova plana {PlanId}", travelPlanId);
            }
        }

      
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var currentRole = GetCurrentUserRole();

                IQueryable<TravelPlan> query = _context.TravelPlans.Where(t => !t.IsDeleted);

                if (currentRole != "Admin")
                {
                    query = query.Where(t => t.UserId == currentUserId);
                }

                var plans = await query
                    .Select(t => new TravelPlanDto
                    {
                        Id = t.Id,
                        UserId = t.UserId,
                        Name = t.Name,
                        Description = t.Description,
                        StartDate = t.StartDate,
                        EndDate = t.EndDate,
                        Budget = t.Budget,
                        Notes = t.Notes,
                        CreatedAt = t.CreatedAt,
                        UpdatedAt = t.UpdatedAt
                    }).ToListAsync();

                _logger.LogInformation($"User {currentUserId} retrieved travel plans");
                return Ok(plans);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving travel plans");
                return StatusCode(500, new { message = "Greška pri preuzimanju putnih planova" });
            }
        }

       
        [HttpGet("my-plans")]
        public async Task<IActionResult> GetMyPlans()
        {
            try
            {
                var currentUserId = GetCurrentUserId();

                var plans = await _context.TravelPlans
                    .Where(t => t.UserId == currentUserId && !t.IsDeleted)
                    .Select(t => new TravelPlanDto
                    {
                        Id = t.Id,
                        UserId = t.UserId,
                        Name = t.Name,
                        Description = t.Description,
                        StartDate = t.StartDate,
                        EndDate = t.EndDate,
                        Budget = t.Budget,
                        Notes = t.Notes,
                        CreatedAt = t.CreatedAt,
                        UpdatedAt = t.UpdatedAt
                    }).ToListAsync();

                _logger.LogInformation($"User {currentUserId} retrieved their own travel plans");
                return Ok(plans);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user's travel plans");
                return StatusCode(500, new { message = "Greška pri preuzimanju vaših putnih planova" });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var currentRole = GetCurrentUserRole();

                var plan = await _context.TravelPlans
                    .Include(t => t.Activities)
                    .Include(t => t.Destinations)
                    .Include(t => t.ChecklistItems)
                    .FirstOrDefaultAsync(t => t.Id == id && !t.IsDeleted);

                if (plan == null)
                {
                    return NotFound(new { message = "Putni plan nije pronađen" });
                }

                if (plan.UserId != currentUserId && currentRole != "Admin")
                {
                    _logger.LogWarning($"User {currentUserId} attempted unauthorized access to plan {id}");
                    return Forbid();
                }

                var dto = new TravelPlanDetailDto
                {
                    Id = plan.Id,
                    UserId = plan.UserId,
                    Name = plan.Name,
                    Description = plan.Description,
                    StartDate = plan.StartDate,
                    EndDate = plan.EndDate,
                    Budget = plan.Budget,
                    Notes = plan.Notes,
                    CreatedAt = plan.CreatedAt,
                    UpdatedAt = plan.UpdatedAt,
                    TotalEstimatedCosts = plan.GetTotalEstimatedCosts(),
                    RemainingBudget = plan.GetRemainingBudget(),
                    DestinationCount = plan.Destinations.Count(d => !d.IsDeleted),
                    ActivityCount = plan.Activities.Count(a => !a.IsDeleted),
                    ChecklistItemCount = plan.ChecklistItems.Count(c => !c.IsDeleted)
                };

                return Ok(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving travel plan {id}");
                return StatusCode(500, new { message = "Greška pri preuzimanju putnog plana" });
            }
        }

        
        [HttpGet("user/{userId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetByUser(int userId)
        {
            try
            {
                var plans = await _context.TravelPlans
                    .Where(t => t.UserId == userId && !t.IsDeleted)
                    .Select(t => new TravelPlanDto
                    {
                        Id = t.Id,
                        UserId = t.UserId,
                        Name = t.Name,
                        Description = t.Description,
                        StartDate = t.StartDate,
                        EndDate = t.EndDate,
                        Budget = t.Budget,
                        Notes = t.Notes,
                        CreatedAt = t.CreatedAt,
                        UpdatedAt = t.UpdatedAt
                    }).ToListAsync();

                _logger.LogInformation($"Admin retrieved travel plans for user {userId}");
                return Ok(plans);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving travel plans for user {userId}");
                return StatusCode(500, new { message = "Greška pri preuzimanju putnih planova korisnika" });
            }
        }

       
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateTravelPlanDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var currentUserId = GetCurrentUserId();

                // Validate dates
                if (dto.EndDate < dto.StartDate)
                {
                    return BadRequest(new { message = "Krajnji datum ne može biti pre početnog datuma." });
                }

                // Validate budget
                if (dto.Budget < 0)
                {
                    return BadRequest(new { message = "Budžet ne može biti negativan." });
                }

                var plan = new TravelPlan
                {
                    UserId = currentUserId,
                    Name = dto.Name,
                    Description = dto.Description,
                    StartDate = dto.StartDate,
                    EndDate = dto.EndDate,
                    Budget = dto.Budget,
                    Notes = dto.Notes,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.TravelPlans.Add(plan);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"User {currentUserId} created travel plan {plan.Id}");

                return CreatedAtAction(nameof(GetById), new { id = plan.Id }, new TravelPlanDto
                {
                    Id = plan.Id,
                    UserId = plan.UserId,
                    Name = plan.Name,
                    Description = plan.Description,
                    StartDate = plan.StartDate,
                    EndDate = plan.EndDate,
                    Budget = plan.Budget,
                    Notes = plan.Notes,
                    CreatedAt = plan.CreatedAt,
                    UpdatedAt = plan.UpdatedAt
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating travel plan");
                return StatusCode(500, new { message = "Greška pri kreiranju putnog plana" });
            }
        }

       
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateTravelPlanDto dto)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var currentRole = GetCurrentUserRole();

                var plan = await _context.TravelPlans.FirstOrDefaultAsync(t => t.Id == id && !t.IsDeleted);
                if (plan == null)
                {
                    return NotFound(new { message = "Putni plan nije pronađen" });
                }

                // Check authorization: owner, admin, or valid EDIT share token
                var hasEditShareAccess = plan.UserId != currentUserId && currentRole != "Admin"
                    && await HasValidEditShareToken(id);

                if (plan.UserId != currentUserId && currentRole != "Admin" && !hasEditShareAccess)
                {
                    _logger.LogWarning($"User {currentUserId} attempted unauthorized update to plan {id}");
                    return Forbid();
                }

                // Update fields if provided
                if (!string.IsNullOrEmpty(dto.Name))
                    plan.Name = dto.Name;

                if (!string.IsNullOrEmpty(dto.Description))
                    plan.Description = dto.Description;

                if (dto.StartDate.HasValue)
                    plan.StartDate = dto.StartDate.Value;

                if (dto.EndDate.HasValue)
                    plan.EndDate = dto.EndDate.Value;

                if (dto.Budget.HasValue)
                    plan.Budget = dto.Budget.Value;

                if (!string.IsNullOrEmpty(dto.Notes))
                    plan.Notes = dto.Notes;

                // Validate dates
                if (plan.EndDate < plan.StartDate)
                {
                    return BadRequest(new { message = "Krajnji datum ne može biti pre početnog datuma." });
                }

                // Validate budget
                if (plan.Budget < 0)
                {
                    return BadRequest(new { message = "Budžet ne može biti negativan." });
                }

                plan.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                _logger.LogInformation($"User {currentUserId} updated travel plan {id}");

                return Ok(new TravelPlanDto
                {
                    Id = plan.Id,
                    UserId = plan.UserId,
                    Name = plan.Name,
                    Description = plan.Description,
                    StartDate = plan.StartDate,
                    EndDate = plan.EndDate,
                    Budget = plan.Budget,
                    Notes = plan.Notes,
                    CreatedAt = plan.CreatedAt,
                    UpdatedAt = plan.UpdatedAt
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating travel plan {id}");
                return StatusCode(500, new { message = "Greška pri ažuriranju putnog plana" });
            }
        }

       
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var currentRole = GetCurrentUserRole();

                var plan = await _context.TravelPlans
                    .Include(t => t.Destinations)
                    .Include(t => t.Activities)
                    .Include(t => t.ChecklistItems)
                    .Include(t => t.ShareTokens)
                    .FirstOrDefaultAsync(t => t.Id == id && !t.IsDeleted);

                if (plan == null)
                {
                    return NotFound(new { message = "Putni plan nije pronađen" });
                }

                // Check authorization: owner or admin
                if (plan.UserId != currentUserId && currentRole != "Admin")
                {
                    _logger.LogWarning($"User {currentUserId} attempted unauthorized delete of plan {id}");
                    return Forbid();
                }

                // Soft delete plan and all related entities
                plan.IsDeleted = true;
                plan.UpdatedAt = DateTime.UtcNow;

                foreach (var destination in plan.Destinations)
                {
                    destination.IsDeleted = true;
                    destination.UpdatedAt = DateTime.UtcNow;
                }

                foreach (var activity in plan.Activities)
                {
                    activity.IsDeleted = true;
                    activity.UpdatedAt = DateTime.UtcNow;
                }

                foreach (var checklist in plan.ChecklistItems)
                {
                    checklist.IsDeleted = true;
                    checklist.UpdatedAt = DateTime.UtcNow;
                }

                foreach (var shareToken in plan.ShareTokens)
                {
                    shareToken.IsDeleted = true;
                }

                await _context.SaveChangesAsync();

                await DeleteExpensesInFinanceService(id);

                _logger.LogInformation($"User {currentUserId} deleted travel plan {id} (soft delete)");

                return Ok(new { message = "Putni plan je uspešno obrisan" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting travel plan {id}");
                return StatusCode(500, new { message = "Greška pri brisanju putnog plana" });
            }
        }
    }
}