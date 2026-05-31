using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TravelService.Data;
using TravelService.DTOs;
using TravelService.Models;

namespace TravelService.Controllers
{
    [ApiController]
    [Route("api/travel-plans/{travelPlanId}/activities")]
    public class ActivitiesController : ControllerBase
    {
        private readonly TravelDbContext _context;

        public ActivitiesController(TravelDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(int travelPlanId)
        {
            var activities = await _context.Activities
                .Where(a => a.TravelPlanId == travelPlanId)
                .Select(a => new ActivityDto
                {
                    Id = a.Id,
                    TravelPlanId = a.TravelPlanId,
                    Name = a.Name,
                    Date = a.Date,
                    Time = a.Time,
                    Location = a.Location,
                    Description = a.Description,
                    EstimatedCost = a.EstimatedCost,
                    Status = a.Status
                }).ToListAsync();

            return Ok(activities);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int travelPlanId, int id)
        {
            var activity = await _context.Activities
                .FirstOrDefaultAsync(a => a.Id == id && a.TravelPlanId == travelPlanId);
            if (activity == null) return NotFound();

            return Ok(new ActivityDto
            {
                Id = activity.Id,
                TravelPlanId = activity.TravelPlanId,
                Name = activity.Name,
                Date = activity.Date,
                Time = activity.Time,
                Location = activity.Location,
                Description = activity.Description,
                EstimatedCost = activity.EstimatedCost,
                Status = activity.Status
            });
        }

        [HttpPost]
        public async Task<IActionResult> Create(int travelPlanId, CreateActivityDto dto)
        {
            var plan = await _context.TravelPlans.FindAsync(travelPlanId);
            if (plan == null) return NotFound(new { message = "Travel plan not found." });

            var activity = new Activity
            {
                TravelPlanId = travelPlanId,
                Name = dto.Name,
                Date = dto.Date,
                Time = dto.Time,
                Location = dto.Location,
                Description = dto.Description,
                EstimatedCost = dto.EstimatedCost,
                Status = dto.Status
            };

            _context.Activities.Add(activity);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { travelPlanId, id = activity.Id }, new ActivityDto
            {
                Id = activity.Id,
                TravelPlanId = activity.TravelPlanId,
                Name = activity.Name,
                Date = activity.Date,
                Time = activity.Time,
                Location = activity.Location,
                Description = activity.Description,
                EstimatedCost = activity.EstimatedCost,
                Status = activity.Status
            });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int travelPlanId, int id, CreateActivityDto dto)
        {
            var activity = await _context.Activities
                .FirstOrDefaultAsync(a => a.Id == id && a.TravelPlanId == travelPlanId);
            if (activity == null) return NotFound();

            activity.Name = dto.Name;
            activity.Date = dto.Date;
            activity.Time = dto.Time;
            activity.Location = dto.Location;
            activity.Description = dto.Description;
            activity.EstimatedCost = dto.EstimatedCost;
            activity.Status = dto.Status;

            await _context.SaveChangesAsync();

            return Ok(new ActivityDto
            {
                Id = activity.Id,
                TravelPlanId = activity.TravelPlanId,
                Name = activity.Name,
                Date = activity.Date,
                Time = activity.Time,
                Location = activity.Location,
                Description = activity.Description,
                EstimatedCost = activity.EstimatedCost,
                Status = activity.Status
            });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int travelPlanId, int id)
        {
            var activity = await _context.Activities
                .FirstOrDefaultAsync(a => a.Id == id && a.TravelPlanId == travelPlanId);
            if (activity == null) return NotFound();

            _context.Activities.Remove(activity);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}