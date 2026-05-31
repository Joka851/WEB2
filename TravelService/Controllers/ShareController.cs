using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TravelService.Data;
using TravelService.DTOs;
using TravelService.Models;

namespace TravelService.Controllers
{
    [ApiController]
    [Route("api/travel-plans/{travelPlanId}/share")]
    public class ShareController : ControllerBase
    {
        private readonly TravelDbContext _context;

        public ShareController(TravelDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> CreateShareToken(int travelPlanId, CreateShareTokenDto dto)
        {
            var plan = await _context.TravelPlans.FindAsync(travelPlanId);
            if (plan == null) return NotFound(new { message = "Travel plan not found." });

            var token = new ShareToken
            {
                TravelPlanId = travelPlanId,
                Token = Guid.NewGuid().ToString(),
                AccessType = dto.AccessType,
                ExpiresAt = DateTime.UtcNow.AddDays(30)
            };

            _context.ShareTokens.Add(token);
            await _context.SaveChangesAsync();

            return Ok(new ShareTokenDto
            {
                Id = token.Id,
                TravelPlanId = token.TravelPlanId,
                Token = token.Token,
                AccessType = token.AccessType,
                ExpiresAt = token.ExpiresAt
            });
        }

        [HttpGet("access/{token}")]
        public async Task<IActionResult> AccessByToken(string token)
        {
            var shareToken = await _context.ShareTokens
                .Include(s => s.TravelPlan)
                .FirstOrDefaultAsync(s => s.Token == token && s.ExpiresAt > DateTime.UtcNow);

            if (shareToken == null)
                return NotFound(new { message = "Token is invalid or expired." });

            return Ok(new
            {
                AccessType = shareToken.AccessType,
                TravelPlan = new TravelPlanDto
                {
                    Id = shareToken.TravelPlan.Id,
                    UserId = shareToken.TravelPlan.UserId,
                    Name = shareToken.TravelPlan.Name,
                    Description = shareToken.TravelPlan.Description,
                    StartDate = shareToken.TravelPlan.StartDate,
                    EndDate = shareToken.TravelPlan.EndDate,
                    Budget = shareToken.TravelPlan.Budget,
                    Notes = shareToken.TravelPlan.Notes,
                    CreatedAt = shareToken.TravelPlan.CreatedAt
                }
            });
        }

        [HttpGet]
        public async Task<IActionResult> GetTokens(int travelPlanId)
        {
            var tokens = await _context.ShareTokens
                .Where(s => s.TravelPlanId == travelPlanId)
                .Select(s => new ShareTokenDto
                {
                    Id = s.Id,
                    TravelPlanId = s.TravelPlanId,
                    Token = s.Token,
                    AccessType = s.AccessType,
                    ExpiresAt = s.ExpiresAt
                }).ToListAsync();

            return Ok(tokens);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int travelPlanId, int id)
        {
            var token = await _context.ShareTokens
                .FirstOrDefaultAsync(s => s.Id == id && s.TravelPlanId == travelPlanId);
            if (token == null) return NotFound();

            _context.ShareTokens.Remove(token);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}