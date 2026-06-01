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
    [Route("api/travel-plans/{travelPlanId}/share")]
    [Authorize]
    public class ShareController : ControllerBase
    {
        private readonly TravelDbContext _context;
        private readonly ILogger<ShareController> _logger;

        public ShareController(TravelDbContext context, ILogger<ShareController> logger)
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
            return plan != null && plan.UserId == currentUserId;
        }

        /// <summary>
        /// Get all share tokens for a travel plan
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAllTokens(int travelPlanId)
        {
            try
            {
                if (!await CanAccessTravelPlan(travelPlanId))
                {
                    return Forbid();
                }

                var tokens = await _context.ShareTokens
                    .Where(s => s.TravelPlanId == travelPlanId && !s.IsDeleted)
                    .Select(s => new ShareTokenDto
                    {
                        Id = s.Id,
                        TravelPlanId = s.TravelPlanId,
                        Token = s.Token,
                        AccessType = s.AccessType,
                        CreatedAt = s.CreatedAt,
                        ExpiresAt = s.ExpiresAt,
                        IsActive = s.IsActive
                    }).ToListAsync();

                return Ok(tokens);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving share tokens for plan {travelPlanId}");
                return StatusCode(500, new { message = "Greška pri preuzimanju share tokena" });
            }
        }

        /// <summary>
        /// Create a new share token
        /// </summary>
        [HttpPost("generate")]
        public async Task<IActionResult> GenerateToken(int travelPlanId, [FromBody] CreateShareTokenDto dto)
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

                // Validate access type
                if (!ShareToken.IsValidAccessType(dto.AccessType))
                {
                    return BadRequest(new { message = "Tip pristupa mora biti VIEW ili EDIT" });
                }

                // Validate expiry date
                if (dto.ExpiresAt <= DateTime.UtcNow)
                {
                    return BadRequest(new { message = "Vreme isteka mora biti u budućnosti" });
                }

                var token = new ShareToken
                {
                    TravelPlanId = travelPlanId,
                    Token = ShareToken.GenerateToken(),
                    AccessType = dto.AccessType,
                    ExpiresAt = dto.ExpiresAt,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                _context.ShareTokens.Add(token);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Share token {token.Id} created for plan {travelPlanId}");

                var baseUrl = $"{Request.Scheme}://{Request.Host}";
                var shareUrl = $"{baseUrl}/share/{token.Token}";

                return Ok(new ShareTokenResponseDto
                {
                    Token = token.Token,
                    Url = shareUrl,
                    ExpiresAt = token.ExpiresAt
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating share token");
                return StatusCode(500, new { message = "Greška pri generisanju share tokena" });
            }
        }

        /// <summary>
        /// Revoke/deactivate a share token
        /// </summary>
        [HttpPost("{tokenId}/revoke")]
        public async Task<IActionResult> RevokeToken(int travelPlanId, int tokenId)
        {
            try
            {
                if (!await CanAccessTravelPlan(travelPlanId))
                {
                    return Forbid();
                }

                var token = await _context.ShareTokens
                    .FirstOrDefaultAsync(s => s.Id == tokenId && s.TravelPlanId == travelPlanId && !s.IsDeleted);

                if (token == null)
                {
                    return NotFound(new { message = "Share token nije pronađen" });
                }

                token.IsActive = false;
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Share token {tokenId} revoked");

                return Ok(new { message = "Share token je uspešno opozvane" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error revoking share token {tokenId}");
                return StatusCode(500, new { message = "Greška pri opozivanju share tokena" });
            }
        }

        /// <summary>
        /// Delete a share token
        /// </summary>
        [HttpDelete("{tokenId}")]
        public async Task<IActionResult> DeleteToken(int travelPlanId, int tokenId)
        {
            try
            {
                if (!await CanAccessTravelPlan(travelPlanId))
                {
                    return Forbid();
                }

                var token = await _context.ShareTokens
                    .FirstOrDefaultAsync(s => s.Id == tokenId && s.TravelPlanId == travelPlanId && !s.IsDeleted);

                if (token == null)
                {
                    return NotFound(new { message = "Share token nije pronađen" });
                }

                // Soft delete
                token.IsDeleted = true;
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Share token {tokenId} deleted (soft delete)");

                return Ok(new { message = "Share token je uspešno obrisan" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting share token {tokenId}");
                return StatusCode(500, new { message = "Greška pri brisanju share tokena" });
            }
        }

        /// <summary>
        /// Validate a share token (can be called without authorization)
        /// </summary>
        [HttpPost("validate")]
        [AllowAnonymous]
        public async Task<IActionResult> ValidateToken([FromBody] ValidateShareTokenDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var token = await _context.ShareTokens
                    .Include(s => s.TravelPlan)
                    .FirstOrDefaultAsync(s => s.Token == dto.Token && !s.IsDeleted);

                if (token == null || !token.IsValid())
                {
                    return Unauthorized(new { message = "Nevažeći ili istekao token" });
                }

                return Ok(new
                {
                    isValid = true,
                    travelPlanId = token.TravelPlanId,
                    accessType = token.AccessType,
                    expiresAt = token.ExpiresAt
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating share token");
                return StatusCode(500, new { message = "Greška pri validaciji share tokena" });
            }
        }

        /// <summary>
        /// Get shared travel plan (accessible via token)
        /// </summary>
        [HttpPost("access/{token}")]
        [AllowAnonymous]
        public async Task<IActionResult> AccessSharedPlan(string token)
        {
            try
            {
                var shareToken = await _context.ShareTokens
                    .Include(s => s.TravelPlan)
                    .ThenInclude(t => t.Activities)
                    .Include(s => s.TravelPlan)
                    .ThenInclude(t => t.Destinations)
                    .FirstOrDefaultAsync(s => s.Token == token && !s.IsDeleted);

                if (shareToken == null || !shareToken.IsValid())
                {
                    return Unauthorized(new { message = "Nevažeći ili istekao token" });
                }

                var plan = shareToken.TravelPlan;

                // For VIEW access, only return summary
                if (shareToken.AccessType == ShareToken.ACCESS_TYPE_VIEW)
                {
                    var summaryDto = new TravelPlanDetailDto
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

                    return Ok(summaryDto);
                }

                // For EDIT access, return full details
                return Ok(new TravelPlanDetailDto
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
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error accessing shared plan");
                return StatusCode(500, new { message = "Greška pri pristupu deljenom planu" });
            }
        }
    }
}
