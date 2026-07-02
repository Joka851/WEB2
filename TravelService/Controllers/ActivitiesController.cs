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
    [Route("api/travel-plans/{travelPlanId}/activities")]
    [Authorize]
    public class ActivitiesController : ControllerBase
    {
        private readonly TravelDbContext _context;
        private readonly ILogger<ActivitiesController> _logger;

        public ActivitiesController(TravelDbContext context, ILogger<ActivitiesController> logger)
        {
            _context = context;
            _logger = logger;
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

        private async Task<bool> CanAccessTravelPlan(int travelPlanId)
        {
            var currentUserId = GetCurrentUserId();
            var currentRole = GetCurrentUserRole();

            if (currentRole == "Admin")
                return true;

            var plan = await _context.TravelPlans.FirstOrDefaultAsync(p => p.Id == travelPlanId && !p.IsDeleted);
            if (plan != null && plan.UserId == currentUserId)
                return true;

            // Nije vlasnik ni admin - proveri da li poseduje važeći EDIT share token za ovaj plan.
            // Korisnik i dalje mora biti ulogovan ([Authorize] na kontroleru to obezbeđuje).
            return await HasValidEditShareToken(travelPlanId);
        }

        /// <summary>
        /// Proverava da li zahtev nosi važeći EDIT share token (header X-Share-Token) za dati putni plan.
        /// </summary>
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

        /// <summary>
        /// Get all activities for a travel plan
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll(int travelPlanId)
        {
            try
            {
                if (!await CanAccessTravelPlan(travelPlanId))
                {
                    _logger.LogWarning($"Unauthorized access attempt to plan {travelPlanId}");
                    return Forbid();
                }

                var activities = await _context.Activities
                    .Where(a => a.TravelPlanId == travelPlanId && !a.IsDeleted)
                    .Select(a => new ActivityDto
                    {
                        Id = a.Id,
                        TravelPlanId = a.TravelPlanId,
                        Name = a.Name,
                        Date = a.Date,
                        Time = a.Time,
                        Location = a.Location,
                        Description = a.Description,
                        EstimatedCost = a.EstimatedCost,
                        Status = a.Status,
                        CreatedAt = a.CreatedAt,
                        UpdatedAt = a.UpdatedAt
                    }).ToListAsync();

                return Ok(activities);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving activities for plan {travelPlanId}");
                return StatusCode(500, new { message = "Greška pri preuzimanju aktivnosti" });
            }
        }

        /// <summary>
        /// Get activity by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int travelPlanId, int id)
        {
            try
            {
                if (!await CanAccessTravelPlan(travelPlanId))
                {
                    return Forbid();
                }

                var activity = await _context.Activities
                    .FirstOrDefaultAsync(a => a.Id == id && a.TravelPlanId == travelPlanId && !a.IsDeleted);

                if (activity == null)
                {
                    return NotFound(new { message = "Aktivnost nije pronađena" });
                }

                return Ok(new ActivityDto
                {
                    Id = activity.Id,
                    TravelPlanId = activity.TravelPlanId,
                    Name = activity.Name,
                    Date = activity.Date,
                    Time = activity.Time,
                    Location = activity.Location,
                    Description = activity.Description,
                    EstimatedCost = activity.EstimatedCost,
                    Status = activity.Status,
                    CreatedAt = activity.CreatedAt,
                    UpdatedAt = activity.UpdatedAt
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving activity {id}");
                return StatusCode(500, new { message = "Greška pri preuzimanju aktivnosti" });
            }
        }

        /// <summary>
        /// Get activities by date range
        /// </summary>
        [HttpGet("by-date")]
        public async Task<IActionResult> GetByDateRange(int travelPlanId, [FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            try
            {
                if (!await CanAccessTravelPlan(travelPlanId))
                {
                    return Forbid();
                }

                var activities = await _context.Activities
                    .Where(a => a.TravelPlanId == travelPlanId &&
                           a.Date >= startDate &&
                           a.Date <= endDate &&
                           !a.IsDeleted)
                    .OrderBy(a => a.Date)
                    .ThenBy(a => a.Time)
                    .Select(a => new ActivityDto
                    {
                        Id = a.Id,
                        TravelPlanId = a.TravelPlanId,
                        Name = a.Name,
                        Date = a.Date,
                        Time = a.Time,
                        Location = a.Location,
                        Description = a.Description,
                        EstimatedCost = a.EstimatedCost,
                        Status = a.Status,
                        CreatedAt = a.CreatedAt,
                        UpdatedAt = a.UpdatedAt
                    }).ToListAsync();

                return Ok(activities);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving activities by date range");
                return StatusCode(500, new { message = "Greška pri preuzimanju aktivnosti" });
            }
        }

        /// <summary>
        /// Create a new activity
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Create(int travelPlanId, [FromBody] CreateActivityDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                if (!await CanAccessTravelPlan(travelPlanId))
                {
                    return Forbid();
                }

                var plan = await _context.TravelPlans.FirstOrDefaultAsync(t => t.Id == travelPlanId && !t.IsDeleted);
                if (plan == null)
                {
                    return NotFound(new { message = "Putni plan nije pronađen." });
                }

                // Validate activity date is within travel plan dates
                if (dto.Date < plan.StartDate || dto.Date > plan.EndDate)
                {
                    return BadRequest(new { message = "Datum aktivnosti mora biti unutar datuma putnog plana." });
                }

                // Validate status
                if (!Activity.IsValidStatus(dto.Status))
                {
                    return BadRequest(new { message = "Nevažeći status aktivnosti. Dozvoljene vrednosti: Planned, Reserved, Completed, Cancelled" });
                }

                var activity = new Activity
                {
                    TravelPlanId = travelPlanId,
                    Name = dto.Name,
                    Date = dto.Date,
                    Time = dto.Time,
                    Location = dto.Location,
                    Description = dto.Description,
                    EstimatedCost = dto.EstimatedCost,
                    Status = dto.Status,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.Activities.Add(activity);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Activity {activity.Id} created for plan {travelPlanId}");

                return CreatedAtAction(nameof(GetById), new { travelPlanId, id = activity.Id }, new ActivityDto
                {
                    Id = activity.Id,
                    TravelPlanId = activity.TravelPlanId,
                    Name = activity.Name,
                    Date = activity.Date,
                    Time = activity.Time,
                    Location = activity.Location,
                    Description = activity.Description,
                    EstimatedCost = activity.EstimatedCost,
                    Status = activity.Status,
                    CreatedAt = activity.CreatedAt,
                    UpdatedAt = activity.UpdatedAt
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating activity");
                return StatusCode(500, new { message = "Greška pri kreiranju aktivnosti" });
            }
        }

        /// <summary>
        /// Update activity
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int travelPlanId, int id, [FromBody] UpdateActivityDto dto)
        {
            try
            {
                if (!await CanAccessTravelPlan(travelPlanId))
                {
                    return Forbid();
                }

                var activity = await _context.Activities
                    .Include(a => a.TravelPlan)
                    .FirstOrDefaultAsync(a => a.Id == id && a.TravelPlanId == travelPlanId && !a.IsDeleted);

                if (activity == null)
                {
                    return NotFound(new { message = "Aktivnost nije pronađena" });
                }

                var plan = activity.TravelPlan;

                // Update fields if provided
                if (!string.IsNullOrEmpty(dto.Name))
                    activity.Name = dto.Name;

                if (dto.Date.HasValue)
                {
                    // Validate date is within travel plan dates
                    if (dto.Date.Value < plan.StartDate || dto.Date.Value > plan.EndDate)
                    {
                        return BadRequest(new { message = "Datum aktivnosti mora biti unutar datuma putnog plana." });
                    }
                    activity.Date = dto.Date.Value;
                }

                if (!string.IsNullOrEmpty(dto.Time))
                    activity.Time = dto.Time;

                if (!string.IsNullOrEmpty(dto.Location))
                    activity.Location = dto.Location;

                if (!string.IsNullOrEmpty(dto.Description))
                    activity.Description = dto.Description;

                if (dto.EstimatedCost.HasValue)
                    activity.EstimatedCost = dto.EstimatedCost.Value;

                if (!string.IsNullOrEmpty(dto.Status))
                {
                    if (!Activity.IsValidStatus(dto.Status))
                    {
                        return BadRequest(new { message = "Nevažeći status aktivnosti. Dozvoljene vrednosti: Planned, Reserved, Completed, Cancelled" });
                    }
                    activity.Status = dto.Status;
                }

                activity.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Activity {id} updated");

                return Ok(new ActivityDto
                {
                    Id = activity.Id,
                    TravelPlanId = activity.TravelPlanId,
                    Name = activity.Name,
                    Date = activity.Date,
                    Time = activity.Time,
                    Location = activity.Location,
                    Description = activity.Description,
                    EstimatedCost = activity.EstimatedCost,
                    Status = activity.Status,
                    CreatedAt = activity.CreatedAt,
                    UpdatedAt = activity.UpdatedAt
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating activity {id}");
                return StatusCode(500, new { message = "Greška pri ažuriranju aktivnosti" });
            }
        }

        /// <summary>
        /// Delete activity (soft delete)
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int travelPlanId, int id)
        {
            try
            {
                if (!await CanAccessTravelPlan(travelPlanId))
                {
                    return Forbid();
                }

                var activity = await _context.Activities
                    .FirstOrDefaultAsync(a => a.Id == id && a.TravelPlanId == travelPlanId && !a.IsDeleted);

                if (activity == null)
                {
                    return NotFound(new { message = "Aktivnost nije pronađena" });
                }

                // Soft delete
                activity.IsDeleted = true;
                activity.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation($"Activity {id} deleted (soft delete)");

                return Ok(new { message = "Aktivnost je uspešno obrisana" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting activity {id}");
                return StatusCode(500, new { message = "Greška pri brisanju aktivnosti" });
            }
        }
    }
}