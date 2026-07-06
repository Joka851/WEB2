using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TravelService.Data;
using TravelService.DTOs;
using TravelService.Models;

namespace TravelService.Controllers
{
    [ApiController]
    [Route("api/travel-plans/{travelPlanId}/destinations")]
    [Authorize]
    public class DestinationsController : ControllerBase
    {
        private readonly TravelDbContext _context;
        private readonly ILogger<DestinationsController> _logger;

        public DestinationsController(TravelDbContext context, ILogger<DestinationsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            return int.Parse(userIdClaim?.Value ?? "0");
        }

        private string GetCurrentUserRole()
        {
            return User.FindFirst(ClaimTypes.Role)?.Value ?? "User";
        }

        private async Task<bool> CanAccessTravelPlan(int travelPlanId)
        {
            var currentUserId = GetCurrentUserId();
            var currentRole = GetCurrentUserRole();

            if (currentRole == "Admin")
                return true;

            var plan = await _context.TravelPlans.FirstOrDefaultAsync(p => p.Id == travelPlanId && !p.IsDeleted);
            if (plan != null && plan.UserId == currentUserId)
                return true;

            
            return await HasValidEditShareToken(travelPlanId);
        }

        /// <summary>
        /// Proverava da li zahtev nosi važeći EDIT share token (header X-Share-Token) za dati putni plan.
        /// </summary>
        private async Task<bool> HasValidEditShareToken(int travelPlanId)
        {
            if (!Request.Headers.TryGetValue("X-Share-Token", out var tokenValue))
                return false;

            var token = tokenValue.ToString();
            if (string.IsNullOrWhiteSpace(token))
                return false;

            var shareToken = await _context.ShareTokens.FirstOrDefaultAsync(s =>
                s.Token == token &&
                s.TravelPlanId == travelPlanId &&
                s.AccessType == ShareToken.ACCESS_TYPE_EDIT &&
                s.IsActive &&
                !s.IsDeleted);

            return shareToken != null && !shareToken.IsExpired();
        }

        /// <summary>
        /// Get all destinations for a travel plan
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll(int travelPlanId)
        {
            try
            {
                if (!await CanAccessTravelPlan(travelPlanId))
                {
                    return Forbid();
                }

                var destinations = await _context.Destinations
                    .Where(d => d.TravelPlanId == travelPlanId && !d.IsDeleted)
                    .OrderBy(d => d.ArrivalDate)
                    .Select(d => new DestinationDto
                    {
                        Id = d.Id,
                        TravelPlanId = d.TravelPlanId,
                        Name = d.Name,
                        Location = d.Location,
                        ArrivalDate = d.ArrivalDate,
                        DepartureDate = d.DepartureDate,
                        Description = d.Description,
                        CreatedAt = d.CreatedAt,
                        UpdatedAt = d.UpdatedAt
                    }).ToListAsync();

                return Ok(destinations);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving destinations for plan {travelPlanId}");
                return StatusCode(500, new { message = "Greška pri preuzimanju destinacija" });
            }
        }

        /// <summary>
        /// Get destination by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int travelPlanId, int id)
        {
            try
            {
                if (!await CanAccessTravelPlan(travelPlanId))
                {
                    return Forbid();
                }

                var destination = await _context.Destinations
                    .FirstOrDefaultAsync(d => d.Id == id && d.TravelPlanId == travelPlanId && !d.IsDeleted);

                if (destination == null)
                {
                    return NotFound(new { message = "Destinacija nije pronađena" });
                }

                return Ok(new DestinationDto
                {
                    Id = destination.Id,
                    TravelPlanId = destination.TravelPlanId,
                    Name = destination.Name,
                    Location = destination.Location,
                    ArrivalDate = destination.ArrivalDate,
                    DepartureDate = destination.DepartureDate,
                    Description = destination.Description,
                    CreatedAt = destination.CreatedAt,
                    UpdatedAt = destination.UpdatedAt
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving destination {id}");
                return StatusCode(500, new { message = "Greška pri preuzimanju destinacije" });
            }
        }

        /// <summary>
        /// Create a new destination
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Create(int travelPlanId, [FromBody] CreateDestinationDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                if (!await CanAccessTravelPlan(travelPlanId))
                {
                    return Forbid();
                }

                var plan = await _context.TravelPlans.FirstOrDefaultAsync(t => t.Id == travelPlanId && !t.IsDeleted);
                if (plan == null)
                {
                    return NotFound(new { message = "Putni plan nije pronađen" });
                }

                // Validate dates are within travel plan dates
                if (dto.ArrivalDate < plan.StartDate || dto.DepartureDate > plan.EndDate)
                {
                    return BadRequest(new { message = "Datumi destinacije moraju biti unutar datuma putnog plana." });
                }

                // Validate departure date >= arrival date
                if (dto.DepartureDate < dto.ArrivalDate)
                {
                    return BadRequest(new { message = "Datum odlaska ne može biti pre datuma dolaska." });
                }

                var destination = new Destination
                {
                    TravelPlanId = travelPlanId,
                    Name = dto.Name,
                    Location = dto.Location,
                    ArrivalDate = dto.ArrivalDate,
                    DepartureDate = dto.DepartureDate,
                    Description = dto.Description,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.Destinations.Add(destination);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Destination {destination.Id} created for plan {travelPlanId}");

                return CreatedAtAction(nameof(GetById), new { travelPlanId, id = destination.Id }, new DestinationDto
                {
                    Id = destination.Id,
                    TravelPlanId = destination.TravelPlanId,
                    Name = destination.Name,
                    Location = destination.Location,
                    ArrivalDate = destination.ArrivalDate,
                    DepartureDate = destination.DepartureDate,
                    Description = destination.Description,
                    CreatedAt = destination.CreatedAt,
                    UpdatedAt = destination.UpdatedAt
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating destination");
                return StatusCode(500, new { message = "Greška pri kreiranju destinacije" });
            }
        }

        /// <summary>
        /// Update destination
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int travelPlanId, int id, [FromBody] UpdateDestinationDto dto)
        {
            try
            {
                if (!await CanAccessTravelPlan(travelPlanId))
                {
                    return Forbid();
                }

                var destination = await _context.Destinations
                    .Include(d => d.TravelPlan)
                    .FirstOrDefaultAsync(d => d.Id == id && d.TravelPlanId == travelPlanId && !d.IsDeleted);

                if (destination == null)
                {
                    return NotFound(new { message = "Destinacija nije pronađena" });
                }

                var plan = destination.TravelPlan;

                // Update fields if provided
                if (!string.IsNullOrEmpty(dto.Name))
                    destination.Name = dto.Name;

                if (!string.IsNullOrEmpty(dto.Location))
                    destination.Location = dto.Location;

                if (dto.ArrivalDate.HasValue)
                {
                    if (dto.ArrivalDate.Value < plan.StartDate)
                    {
                        return BadRequest(new { message = "Datum dolaska mora biti unutar datuma putnog plana." });
                    }
                    destination.ArrivalDate = dto.ArrivalDate.Value;
                }

                if (dto.DepartureDate.HasValue)
                {
                    if (dto.DepartureDate.Value > plan.EndDate)
                    {
                        return BadRequest(new { message = "Datum odlaska mora biti unutar datuma putnog plana." });
                    }
                    destination.DepartureDate = dto.DepartureDate.Value;
                }

                if (!string.IsNullOrEmpty(dto.Description))
                    destination.Description = dto.Description;

                // Validate departure date >= arrival date
                if (destination.DepartureDate < destination.ArrivalDate)
                {
                    return BadRequest(new { message = "Datum odlaska ne može biti pre datuma dolaska." });
                }

                destination.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Destination {id} updated");

                return Ok(new DestinationDto
                {
                    Id = destination.Id,
                    TravelPlanId = destination.TravelPlanId,
                    Name = destination.Name,
                    Location = destination.Location,
                    ArrivalDate = destination.ArrivalDate,
                    DepartureDate = destination.DepartureDate,
                    Description = destination.Description,
                    CreatedAt = destination.CreatedAt,
                    UpdatedAt = destination.UpdatedAt
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating destination {id}");
                return StatusCode(500, new { message = "Greška pri ažuriranju destinacije" });
            }
        }

        /// <summary>
        /// Delete destination (soft delete)
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int travelPlanId, int id)
        {
            try
            {
                if (!await CanAccessTravelPlan(travelPlanId))
                {
                    return Forbid();
                }

                var destination = await _context.Destinations
                    .FirstOrDefaultAsync(d => d.Id == id && d.TravelPlanId == travelPlanId && !d.IsDeleted);

                if (destination == null)
                {
                    return NotFound(new { message = "Destinacija nije pronađena" });
                }

                // Soft delete
                destination.IsDeleted = true;
                destination.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation($"Destination {id} deleted (soft delete)");

                return Ok(new { message = "Destinacija je uspešno obrisana" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting destination {id}");
                return StatusCode(500, new { message = "Greška pri brisanju destinacije" });
            }
        }
    }
}