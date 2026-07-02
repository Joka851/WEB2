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
    [Route("api/travel-plans/{travelPlanId}/checklists")]
    [Authorize]
    public class CheckListController : ControllerBase
    {
        private readonly TravelDbContext _context;
        private readonly ILogger<CheckListController> _logger;

        public CheckListController(TravelDbContext context, ILogger<CheckListController> logger)
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
        /// Get all checklist items for a travel plan
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll(int travelPlanId)
        {
            try
            {
                if (!await CanAccessTravelPlan(travelPlanId))
                {
                    return Forbid();
                }

                var items = await _context.ChecklistItems
                    .Where(c => c.TravelPlanId == travelPlanId && !c.IsDeleted)
                    .OrderBy(c => c.IsCompleted)
                    .ThenBy(c => c.CreatedAt)
                    .Select(c => new ChecklistItemDto
                    {
                        Id = c.Id,
                        TravelPlanId = c.TravelPlanId,
                        Name = c.Name,
                        IsCompleted = c.IsCompleted,
                        CreatedAt = c.CreatedAt,
                        UpdatedAt = c.UpdatedAt
                    }).ToListAsync();

                return Ok(items);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving checklist items for plan {travelPlanId}");
                return StatusCode(500, new { message = "Greška pri preuzimanju checklist stavki" });
            }
        }

        /// <summary>
        /// Get checklist item by ID
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

                var item = await _context.ChecklistItems
                    .FirstOrDefaultAsync(c => c.Id == id && c.TravelPlanId == travelPlanId && !c.IsDeleted);

                if (item == null)
                {
                    return NotFound(new { message = "Checklist stavka nije pronađena" });
                }

                return Ok(new ChecklistItemDto
                {
                    Id = item.Id,
                    TravelPlanId = item.TravelPlanId,
                    Name = item.Name,
                    IsCompleted = item.IsCompleted,
                    CreatedAt = item.CreatedAt,
                    UpdatedAt = item.UpdatedAt
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving checklist item {id}");
                return StatusCode(500, new { message = "Greška pri preuzimanju checklist stavke" });
            }
        }

        /// <summary>
        /// Create a new checklist item
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Create(int travelPlanId, [FromBody] CreateChecklistItemDto dto)
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
                    return NotFound(new { message = "Putni plan nije pronađen" });
                }

                var item = new ChecklistItem
                {
                    TravelPlanId = travelPlanId,
                    Name = dto.Name,
                    IsCompleted = dto.IsCompleted,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.ChecklistItems.Add(item);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Checklist item {item.Id} created for plan {travelPlanId}");

                return CreatedAtAction(nameof(GetById), new { travelPlanId, id = item.Id }, new ChecklistItemDto
                {
                    Id = item.Id,
                    TravelPlanId = item.TravelPlanId,
                    Name = item.Name,
                    IsCompleted = item.IsCompleted,
                    CreatedAt = item.CreatedAt,
                    UpdatedAt = item.UpdatedAt
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating checklist item");
                return StatusCode(500, new { message = "Greška pri kreiranju checklist stavke" });
            }
        }

        /// <summary>
        /// Update checklist item
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int travelPlanId, int id, [FromBody] UpdateChecklistItemDto dto)
        {
            try
            {
                if (!await CanAccessTravelPlan(travelPlanId))
                {
                    return Forbid();
                }

                var item = await _context.ChecklistItems
                    .FirstOrDefaultAsync(c => c.Id == id && c.TravelPlanId == travelPlanId && !c.IsDeleted);

                if (item == null)
                {
                    return NotFound(new { message = "Checklist stavka nije pronađena" });
                }

                if (!string.IsNullOrEmpty(dto.Name))
                    item.Name = dto.Name;

                if (dto.IsCompleted.HasValue)
                    item.IsCompleted = dto.IsCompleted.Value;

                item.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Checklist item {id} updated");

                return Ok(new ChecklistItemDto
                {
                    Id = item.Id,
                    TravelPlanId = item.TravelPlanId,
                    Name = item.Name,
                    IsCompleted = item.IsCompleted,
                    CreatedAt = item.CreatedAt,
                    UpdatedAt = item.UpdatedAt
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating checklist item {id}");
                return StatusCode(500, new { message = "Greška pri ažuriranju checklist stavke" });
            }
        }

        /// <summary>
        /// Toggle the IsCompleted status of a checklist item
        /// </summary>
        [HttpPut("{id}/toggle")]
        public async Task<IActionResult> Toggle(int travelPlanId, int id)
        {
            try
            {
                if (!await CanAccessTravelPlan(travelPlanId))
                {
                    return Forbid();
                }

                var item = await _context.ChecklistItems
                    .FirstOrDefaultAsync(c => c.Id == id && c.TravelPlanId == travelPlanId && !c.IsDeleted);

                if (item == null)
                {
                    return NotFound(new { message = "Checklist stavka nije pronađena" });
                }

                item.IsCompleted = !item.IsCompleted;
                item.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation($"Checklist item {id} toggled to {item.IsCompleted}");

                return Ok(new ChecklistItemDto
                {
                    Id = item.Id,
                    TravelPlanId = item.TravelPlanId,
                    Name = item.Name,
                    IsCompleted = item.IsCompleted,
                    CreatedAt = item.CreatedAt,
                    UpdatedAt = item.UpdatedAt
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error toggling checklist item {id}");
                return StatusCode(500, new { message = "Greška pri ažuriranju checklist stavke" });
            }
        }

        /// <summary>
        /// Delete checklist item (soft delete)
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

                var item = await _context.ChecklistItems
                    .FirstOrDefaultAsync(c => c.Id == id && c.TravelPlanId == travelPlanId && !c.IsDeleted);

                if (item == null)
                {
                    return NotFound(new { message = "Checklist stavka nije pronađena" });
                }

                item.IsDeleted = true;
                item.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation($"Checklist item {id} deleted (soft delete)");

                return Ok(new { message = "Checklist stavka je uspešno obrisana" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting checklist item {id}");
                return StatusCode(500, new { message = "Greška pri brisanju checklist stavke" });
            }
        }
    }
}