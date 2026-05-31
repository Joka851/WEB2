using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TravelService.Data;
using TravelService.DTOs;
using TravelService.Models;

namespace TravelService.Controllers
{
    [ApiController]
    [Route("api/travel-plans/{travelPlanId}/destinations")]
    public class DestinationsController : ControllerBase
    {
        private readonly TravelDbContext _context;

        public DestinationsController(TravelDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(int travelPlanId)
        {
            var destinations = await _context.Destinations
                .Where(d => d.TravelPlanId == travelPlanId)
                .Select(d => new DestinationDto
                {
                    Id = d.Id,
                    TravelPlanId = d.TravelPlanId,
                    Name = d.Name,
                    Location = d.Location,
                    ArrivalDate = d.ArrivalDate,
                    DepartureDate = d.DepartureDate,
                    Description = d.Description
                }).ToListAsync();

            return Ok(destinations);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int travelPlanId, int id)
        {
            var destination = await _context.Destinations
                .FirstOrDefaultAsync(d => d.Id == id && d.TravelPlanId == travelPlanId);
            if (destination == null) return NotFound();

            return Ok(new DestinationDto
            {
                Id = destination.Id,
                TravelPlanId = destination.TravelPlanId,
                Name = destination.Name,
                Location = destination.Location,
                ArrivalDate = destination.ArrivalDate,
                DepartureDate = destination.DepartureDate,
                Description = destination.Description
            });
        }

        [HttpPost]
        public async Task<IActionResult> Create(int travelPlanId, CreateDestinationDto dto)
        {
            var plan = await _context.TravelPlans.FindAsync(travelPlanId);
            if (plan == null) return NotFound(new { message = "Travel plan not found." });

            if (dto.DepartureDate < dto.ArrivalDate)
                return BadRequest(new { message = "Departure date cannot be before arrival date." });

            var destination = new Destination
            {
                TravelPlanId = travelPlanId,
                Name = dto.Name,
                Location = dto.Location,
                ArrivalDate = dto.ArrivalDate,
                DepartureDate = dto.DepartureDate,
                Description = dto.Description
            };

            _context.Destinations.Add(destination);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { travelPlanId, id = destination.Id }, new DestinationDto
            {
                Id = destination.Id,
                TravelPlanId = destination.TravelPlanId,
                Name = destination.Name,
                Location = destination.Location,
                ArrivalDate = destination.ArrivalDate,
                DepartureDate = destination.DepartureDate,
                Description = destination.Description
            });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int travelPlanId, int id, CreateDestinationDto dto)
        {
            var destination = await _context.Destinations
                .FirstOrDefaultAsync(d => d.Id == id && d.TravelPlanId == travelPlanId);
            if (destination == null) return NotFound();

            if (dto.DepartureDate < dto.ArrivalDate)
                return BadRequest(new { message = "Departure date cannot be before arrival date." });

            destination.Name = dto.Name;
            destination.Location = dto.Location;
            destination.ArrivalDate = dto.ArrivalDate;
            destination.DepartureDate = dto.DepartureDate;
            destination.Description = dto.Description;

            await _context.SaveChangesAsync();

            return Ok(new DestinationDto
            {
                Id = destination.Id,
                TravelPlanId = destination.TravelPlanId,
                Name = destination.Name,
                Location = destination.Location,
                ArrivalDate = destination.ArrivalDate,
                DepartureDate = destination.DepartureDate,
                Description = destination.Description
            });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int travelPlanId, int id)
        {
            var destination = await _context.Destinations
                .FirstOrDefaultAsync(d => d.Id == id && d.TravelPlanId == travelPlanId);
            if (destination == null) return NotFound();

            _context.Destinations.Remove(destination);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}