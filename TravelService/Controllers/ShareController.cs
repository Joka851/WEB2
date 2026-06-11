using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TravelService.Data;

namespace TravelService.Controllers
{
    /// <summary>
    /// Javni endpoint za pristup dijeljenom planu putem tokena.
    /// Ruta: GET /api/share/access/{token}
    /// Ne zahtijeva autentifikaciju.
    /// </summary>
    [Route("api/share")]
    [ApiController]
    public class SharePublicController : ControllerBase
    {
        private readonly TravelDbContext _context;

        public SharePublicController(TravelDbContext context)
        {
            _context = context;
        }

        // GET: api/share/access/{token}
        [HttpGet("access/{token}")]
        [AllowAnonymous]
        public async Task<IActionResult> AccessByToken(string token)
        {
            var shareToken = await _context.ShareTokens
                .Include(s => s.TravelPlan)
                    .ThenInclude(tp => tp.Activities)
                .Include(s => s.TravelPlan)
                    .ThenInclude(tp => tp.ChecklistItems)
                .FirstOrDefaultAsync(s => s.Token == token);

            if (shareToken == null)
                return NotFound(new { message = "Token nije pronađen." });

            if (shareToken.ExpiresAt < DateTime.UtcNow)
                return BadRequest(new { message = "Token je istekao." });

            return Ok(shareToken.TravelPlan);
        }
    }
}