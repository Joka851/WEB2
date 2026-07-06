using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TravelService.Data;

namespace TravelService.Controllers
{
    
    [ApiController]
    [Route("api/internal/users")]
    [AllowAnonymous]
    public class InternalController : ControllerBase
    {
        private readonly TravelDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<InternalController> _logger;

        public InternalController(
            TravelDbContext context,
            IConfiguration configuration,
            IHttpClientFactory httpClientFactory,
            ILogger<InternalController> logger)
        {
            _context = context;
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        private bool IsValidInternalKey()
        {
            if (!Request.Headers.TryGetValue("X-Internal-Api-Key", out var providedKey))
                return false;

            var expectedKey = _configuration["Internal:ApiKey"];
            return !string.IsNullOrEmpty(expectedKey) && providedKey == expectedKey;
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
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška pri pozivanju FinanceService za cascade brisanje troškova plana {PlanId}", travelPlanId);
            }
        }

        
        [HttpDelete("{userId}/travel-plans")]
        public async Task<IActionResult> DeletePlansForUser(int userId)
        {
            if (!IsValidInternalKey())
            {
                _logger.LogWarning("Neautorizovan pokušaj pristupa internom endpoint-u (travel-plans) za korisnika {UserId}", userId);
                return Unauthorized(new { message = "Nevažeći interni API ključ." });
            }

            try
            {
                var plans = await _context.TravelPlans
                    .Include(t => t.Destinations)
                    .Include(t => t.Activities)
                    .Include(t => t.ChecklistItems)
                    .Include(t => t.ShareTokens)
                    .Where(t => t.UserId == userId && !t.IsDeleted)
                    .ToListAsync();

                foreach (var plan in plans)
                {
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
                }

                await _context.SaveChangesAsync();

                // Kaskadno obriši troškove u FinanceService za svaki obrisani plan
                foreach (var plan in plans)
                {
                    await DeleteExpensesInFinanceService(plan.Id);
                }

                _logger.LogInformation("Cascade delete: obrisano {Count} planova za korisnika {UserId}", plans.Count, userId);
                return Ok(new { message = "Planovi korisnika obrisani.", count = plans.Count });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška pri cascade brisanju planova za korisnika {UserId}", userId);
                return StatusCode(500, new { message = "Greška pri brisanju planova korisnika." });
            }
        }
    }
}