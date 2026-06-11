using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TravelService.Data;
using TravelService.Models;

namespace TravelService.Controllers
{
    [Route("api/travel-plans/{travelPlanId}/checklists")]
    [ApiController]
    [Authorize]
    public class CheckListController : ControllerBase
    {
        private readonly TravelDbContext _context;

        public CheckListController(TravelDbContext context)
        {
            _context = context;
        }

        // GET: api/travel-plans/{travelPlanId}/checklists
        [HttpGet]
        public async Task<IActionResult> GetAll(int travelPlanId)
        {
            var items = await _context.ChecklistItems
                .Where(c => c.TravelPlanId == travelPlanId)
                .ToListAsync();
            return Ok(items);
        }

        // GET: api/travel-plans/{travelPlanId}/checklists/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int travelPlanId, int id)
        {
            var item = await _context.ChecklistItems
                .FirstOrDefaultAsync(c => c.Id == id && c.TravelPlanId == travelPlanId);
            if (item == null) return NotFound();
            return Ok(item);
        }

        // POST: api/travel-plans/{travelPlanId}/checklists
        [HttpPost]
        public async Task<IActionResult> Create(int travelPlanId, [FromBody] ChecklistItem item)
        {
            item.TravelPlanId = travelPlanId;
            item.IsCompleted = false;
            _context.ChecklistItems.Add(item);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { travelPlanId, id = item.Id }, item);
        }

        // PUT: api/travel-plans/{travelPlanId}/checklists/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int travelPlanId, int id, [FromBody] ChecklistItem updated)
        {
            var item = await _context.ChecklistItems
                .FirstOrDefaultAsync(c => c.Id == id && c.TravelPlanId == travelPlanId);
            if (item == null) return NotFound();

            item.Name = updated.Name;
            item.IsCompleted = updated.IsCompleted;
            await _context.SaveChangesAsync();
            return Ok(item);
        }

        // PUT: api/travel-plans/{travelPlanId}/checklists/{id}/toggle
        [HttpPut("{id}/toggle")]
        public async Task<IActionResult> Toggle(int travelPlanId, int id)
        {
            var item = await _context.ChecklistItems
                .FirstOrDefaultAsync(c => c.Id == id && c.TravelPlanId == travelPlanId);
            if (item == null) return NotFound();

            item.IsCompleted = !item.IsCompleted;
            await _context.SaveChangesAsync();
            return Ok(item);
        }

        // DELETE: api/travel-plans/{travelPlanId}/checklists/{id}
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
