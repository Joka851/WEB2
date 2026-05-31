using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TravelService.Data;
using TravelService.DTOs;
using TravelService.Models;

namespace TravelService.Controllers
{
    [ApiController]
    [Route("api/travel-plans/{travelPlanId}/checklist")]
    public class ChecklistController : ControllerBase
    {
        private readonly TravelDbContext _context;

        public ChecklistController(TravelDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(int travelPlanId)
        {
            var items = await _context.ChecklistItems
                .Where(c => c.TravelPlanId == travelPlanId)
                .Select(c => new ChecklistItemDto
                {
                    Id = c.Id,
                    TravelPlanId = c.TravelPlanId,
                    Name = c.Name,
                    IsCompleted = c.IsCompleted
                }).ToListAsync();

            return Ok(items);
        }

        [HttpPost]
        public async Task<IActionResult> Create(int travelPlanId, CreateChecklistItemDto dto)
        {
            var plan = await _context.TravelPlans.FindAsync(travelPlanId);
            if (plan == null) return NotFound(new { message = "Travel plan not found." });

            var item = new ChecklistItem
            {
                TravelPlanId = travelPlanId,
                Name = dto.Name,
                IsCompleted = false
            };

            _context.ChecklistItems.Add(item);
            await _context.SaveChangesAsync();

            return Ok(new ChecklistItemDto
            {
                Id = item.Id,
                TravelPlanId = item.TravelPlanId,
                Name = item.Name,
                IsCompleted = item.IsCompleted
            });
        }

        [HttpPut("{id}/toggle")]
        public async Task<IActionResult> Toggle(int travelPlanId, int id)
        {
            var item = await _context.ChecklistItems
                .FirstOrDefaultAsync(c => c.Id == id && c.TravelPlanId == travelPlanId);
            if (item == null) return NotFound();

            item.IsCompleted = !item.IsCompleted;
            await _context.SaveChangesAsync();

            return Ok(new ChecklistItemDto
            {
                Id = item.Id,
                TravelPlanId = item.TravelPlanId,
                Name = item.Name,
                IsCompleted = item.IsCompleted
            });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int travelPlanId, int id)
        {
            var item = await _context.ChecklistItems
                .FirstOrDefaultAsync(c => c.Id == id && c.TravelPlanId == travelPlanId);
            if (item == null) return NotFound();

            _context.ChecklistItems.Remove(item);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}