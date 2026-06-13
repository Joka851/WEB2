using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TravelService.Data;

namespace TravelService.Controllers
{
    [Route("api/share")]
    [ApiController]
    public class SharePublicController : ControllerBase
    {
        private readonly TravelDbContext _context;
        public SharePublicController(TravelDbContext context)
        {
            _context = context;
        }

        [HttpGet("access/{token}")]
        [AllowAnonymous]
        public async Task<IActionResult> AccessByToken(string token)
        {
            var shareToken = await _context.ShareTokens
                .Include(s => s.TravelPlan)
                    .ThenInclude(tp => tp.Activities.Where(a => !a.IsDeleted))
                .Include(s => s.TravelPlan)
                    .ThenInclude(tp => tp.Destinations.Where(d => !d.IsDeleted))
                .Include(s => s.TravelPlan)
                    .ThenInclude(tp => tp.ChecklistItems.Where(c => !c.IsDeleted))
                .FirstOrDefaultAsync(s => s.Token == token && !s.IsDeleted);

            if (shareToken == null)
                return NotFound(new { message = "Token nije pronađen." });

            if (shareToken.ExpiresAt != default && shareToken.ExpiresAt < DateTime.UtcNow)
                return BadRequest(new { message = "Token je istekao." });

            var plan = shareToken.TravelPlan;

            return Ok(new
            {
                accessType = shareToken.AccessType,
                travelPlan = new
                {
                    plan.Id,
                    plan.Name,
                    plan.Description,
                    plan.StartDate,
                    plan.EndDate,
                    plan.Budget,
                    plan.Notes,
                    destinations = plan.Destinations.Select(d => new
                    {
                        d.Id,
                        d.Name,
                        d.Location,
                        d.ArrivalDate,
                        d.DepartureDate,
                        d.Description
                    }),
                    activities = plan.Activities.Select(a => new
                    {
                        a.Id,
                        a.Name,
                        a.Date,
                        a.Time,
                        a.Location,
                        a.Description,
                        a.EstimatedCost,
                        a.Status
                    }),
                    checklistItems = plan.ChecklistItems.Select(c => new
                    {
                        c.Id,
                        c.Name,
                        c.IsCompleted
                    })
                }
            });
        }
    }
}