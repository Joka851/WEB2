using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TravelService.Data;
using TravelService.Models;
using TravelService.DTOs;

namespace TravelService.Controllers
{
    [ApiController]
    [Route("api/travel-plans/{travelPlanId}/share")]
    [Authorize]
    public class ShareTokensController : ControllerBase
    {
        private readonly TravelDbContext _context;

        public ShareTokensController(TravelDbContext context)
        {
            _context = context;
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            return int.Parse(userIdClaim?.Value ?? "0");
        }

        private async Task<bool> CanAccessTravelPlan(int travelPlanId)
        {
            var currentUserId = GetCurrentUserId();
            var currentRole = User.FindFirst(ClaimTypes.Role)?.Value ?? "User";
            if (currentRole == "Admin") return true;
            var plan = await _context.TravelPlans.FirstOrDefaultAsync(p => p.Id == travelPlanId && !p.IsDeleted);
            return plan != null && plan.UserId == currentUserId;
        }

        // GET: api/travel-plans/{travelPlanId}/share
        [HttpGet]
        public async Task<IActionResult> GetAll(int travelPlanId)
        {
            if (!await CanAccessTravelPlan(travelPlanId))
                return Forbid();

            var tokens = await _context.ShareTokens
                .Where(s => s.TravelPlanId == travelPlanId && !s.IsDeleted)
                .Select(s => new
                {
                    s.Id,
                    s.Token,
                    s.AccessType,
                    s.ExpiresAt,
                    s.CreatedAt
                }).ToListAsync();

            return Ok(tokens);
        }

        // POST: api/travel-plans/{travelPlanId}/share/generate
        [HttpPost("generate")]
        public async Task<IActionResult> Generate(int travelPlanId, [FromBody] CreateShareTokenDto dto)
        {
            if (!await CanAccessTravelPlan(travelPlanId))
                return Forbid();

            var plan = await _context.TravelPlans.FirstOrDefaultAsync(p => p.Id == travelPlanId && !p.IsDeleted);
            if (plan == null)
                return NotFound(new { message = "Plan nije pronađen." });

            var shareToken = new ShareToken
            {
                TravelPlanId = travelPlanId,
                Token = Guid.NewGuid().ToString("N"),
                AccessType = dto.AccessType,
                ExpiresAt = dto.ExpiresAt,
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false
            };

            _context.ShareTokens.Add(shareToken);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                shareToken.Id,
                shareToken.Token,
                shareToken.AccessType,
                shareToken.ExpiresAt,
                shareToken.CreatedAt
            });
        }

        // DELETE: api/travel-plans/{travelPlanId}/share/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int travelPlanId, int id)
        {
            if (!await CanAccessTravelPlan(travelPlanId))
                return Forbid();

            var token = await _context.ShareTokens
                .FirstOrDefaultAsync(s => s.Id == id && s.TravelPlanId == travelPlanId && !s.IsDeleted);

            if (token == null)
                return NotFound(new { message = "Token nije pronađen." });

            token.IsDeleted = true;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Token obrisan." });
        }
    }

}