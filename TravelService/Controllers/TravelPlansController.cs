using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TravelService.Data;
using TravelService.DTOs;
using TravelService.Models;

namespace TravelService.Controllers
{
    [ApiController]
    [Route("api/travel-plans")]
    public class TravelPlansController : ControllerBase
    {
        private readonly TravelDbContext _context;

        public TravelPlansController(TravelDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var plans = await _context.TravelPlans
                .Select(t => new TravelPlanDto
                {
                    Id = t.Id,
                    UserId = t.UserId,
                    Name = t.Name,
                    Description = t.Description,
                    StartDate = t.StartDate,
                    EndDate = t.EndDate,
                    Budget = t.Budget,
                    Notes = t.Notes,
                    CreatedAt = t.CreatedAt
                }).ToListAsync();

            return Ok(plans);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var plan = await _context.TravelPlans.FindAsync(id);
            if (plan == null) return NotFound();

            return Ok(new TravelPlanDto
            {
                Id = plan.Id,
                UserId = plan.UserId,
                Name = plan.Name,
                Description = plan.Description,
                StartDate = plan.StartDate,
                EndDate = plan.EndDate,
                Budget = plan.Budget,
                Notes = plan.Notes,
                CreatedAt = plan.CreatedAt
            });
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetByUser(int userId)
        {
            var plans = await _context.TravelPlans
                .Where(t => t.UserId == userId)
                .Select(t => new TravelPlanDto
                {
                    Id = t.Id,
                    UserId = t.UserId,
                    Name = t.Name,
                    Description = t.Description,
                    StartDate = t.StartDate,
                    EndDate = t.EndDate,
                    Budget = t.Budget,
                    Notes = t.Notes,
                    CreatedAt = t.CreatedAt
                }).ToListAsync();

            return Ok(plans);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateTravelPlanDto dto)
        {
            if (dto.EndDate < dto.StartDate)
                return BadRequest(new { message = "End date cannot be before start date." });

            if (dto.Budget < 0)
                return BadRequest(new { message = "Budget cannot be negative." });

            var plan = new TravelPlan
            {
                UserId = dto.UserId,
                Name = dto.Name,
                Description = dto.Description,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                Budget = dto.Budget,
                Notes = dto.Notes
            };

            _context.TravelPlans.Add(plan);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = plan.Id }, new TravelPlanDto
            {
                Id = plan.Id,
                UserId = plan.UserId,
                Name = plan.Name,
                Description = plan.Description,
                StartDate = plan.StartDate,
                EndDate = plan.EndDate,
                Budget = plan.Budget,
                Notes = plan.Notes,
                CreatedAt = plan.CreatedAt
            });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, CreateTravelPlanDto dto)
        {
            var plan = await _context.TravelPlans.FindAsync(id);
            if (plan == null) return NotFound();

            if (dto.EndDate < dto.StartDate)
                return BadRequest(new { message = "End date cannot be before start date." });

            if (dto.Budget < 0)
                return BadRequest(new { message = "Budget cannot be negative." });

            plan.Name = dto.Name;
            plan.Description = dto.Description;
            plan.StartDate = dto.StartDate;
            plan.EndDate = dto.EndDate;
            plan.Budget = dto.Budget;
            plan.Notes = dto.Notes;

            await _context.SaveChangesAsync();
            return Ok(new TravelPlanDto
            {
                Id = plan.Id,
                UserId = plan.UserId,
                Name = plan.Name,
                Description = plan.Description,
                StartDate = plan.StartDate,
                EndDate = plan.EndDate,
                Budget = plan.Budget,
                Notes = plan.Notes,
                CreatedAt = plan.CreatedAt
            });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var plan = await _context.TravelPlans.FindAsync(id);
            if (plan == null) return NotFound();

            _context.TravelPlans.Remove(plan);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}