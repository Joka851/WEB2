using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FinanceService.Data;

namespace FinanceService.Controllers
{
    /// <summary>
    /// Interni endpoint-i koje pozivaju drugi mikroservisi direktno (server-to-server),
    /// ne prolaze kroz Gateway i ne koriste JWT autentikaciju korisnika.
    /// Zaštićeni su deljenim internim API ključem koji poznaju samo servisi u sistemu.
    /// </summary>
    [ApiController]
    [Route("api/internal/travel-plans")]
    [AllowAnonymous]
    public class InternalController : ControllerBase
    {
        private readonly FinanceDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly ILogger<InternalController> _logger;

        public InternalController(FinanceDbContext context, IConfiguration configuration, ILogger<InternalController> logger)
        {
            _context = context;
            _configuration = configuration;
            _logger = logger;
        }

        private bool IsValidInternalKey()
        {
            if (!Request.Headers.TryGetValue("X-Internal-Api-Key", out var providedKey))
                return false;

            var expectedKey = _configuration["Internal:ApiKey"];
            return !string.IsNullOrEmpty(expectedKey) && providedKey == expectedKey;
        }

        /// <summary>
        /// Briše (soft delete) sve troškove vezane za jedan putni plan.
        /// Poziva ga TravelService kada korisnik ili admin obriše putni plan (cascade delete).
        /// </summary>
        [HttpDelete("{travelPlanId}/expenses")]
        public async Task<IActionResult> DeleteExpensesForPlan(int travelPlanId)
        {
            if (!IsValidInternalKey())
            {
                _logger.LogWarning("Neautorizovan pokušaj pristupa internom endpoint-u (expenses) za plan {PlanId}", travelPlanId);
                return Unauthorized(new { message = "Nevažeći interni API ključ." });
            }

            try
            {
                var expenses = await _context.Expenses
                    .Where(e => e.TravelPlanId == travelPlanId && !e.IsDeleted)
                    .ToListAsync();

                foreach (var expense in expenses)
                {
                    expense.IsDeleted = true;
                    expense.UpdatedAt = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation("Cascade delete: obrisano {Count} troškova za plan {PlanId}", expenses.Count, travelPlanId);
                return Ok(new { message = "Troškovi obrisani.", count = expenses.Count });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška pri cascade brisanju troškova za plan {PlanId}", travelPlanId);
                return StatusCode(500, new { message = "Greška pri brisanju troškova." });
            }
        }
    }
}