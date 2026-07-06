using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using UserService.Data;
using UserService.DTOs;
using UserService.Models;

namespace UserService.Controllers
{
    [ApiController]
    [Route("api/users")]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly UserDbContext _context;
        private readonly ILogger<UsersController> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public UsersController(
            UserDbContext context,
            ILogger<UsersController> logger,
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration)
        {
            _context = context;
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
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

       
        private async Task DeleteTravelPlansForUser(int userId)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("TravelService");
                var internalKey = _configuration["Internal:ApiKey"];
                var request = new HttpRequestMessage(HttpMethod.Delete, $"/api/internal/users/{userId}/travel-plans");
                request.Headers.Add("X-Internal-Api-Key", internalKey);

                var response = await client.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning(
                        "TravelService je vratio {StatusCode} pri cascade brisanju planova za korisnika {UserId}",
                        response.StatusCode, userId);
                }
                else
                {
                    _logger.LogInformation("Cascade: TravelService obrisao planove za korisnika {UserId}", userId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška pri pozivanju TravelService za cascade brisanje planova korisnika {UserId}", userId);
            }
        }

      
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var users = await _context.Users
                    .Where(u => !u.IsDeleted)
                    .Select(u => new UserDto
                    {
                        Id = u.Id,
                        FirstName = u.FirstName,
                        LastName = u.LastName,
                        Email = u.Email,
                        Role = u.Role,
                        CreatedAt = u.CreatedAt,
                        UpdatedAt = u.UpdatedAt,
                        IsActive = u.IsActive,
                        IsDeleted = u.IsDeleted
                    }).ToListAsync();

                _logger.LogInformation("Admin retrieved all users");
                return Ok(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all users");
                return StatusCode(500, new { message = "Greška pri preuzimanju korisnika" });
            }
        }

      
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var currentRole = GetCurrentUserRole();

                // User can only view their own profile, except Admin
                if (currentUserId != id && currentRole != "Admin")
                {
                    _logger.LogWarning($"User {currentUserId} attempted to view profile of user {id}");
                    return Forbid();
                }

                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id && !u.IsDeleted);
                if (user == null)
                {
                    return NotFound(new { message = "Korisnik nije pronađen" });
                }

                return Ok(new UserDto
                {
                    Id = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    Role = user.Role,
                    CreatedAt = user.CreatedAt,
                    UpdatedAt = user.UpdatedAt,
                    IsActive = user.IsActive,
                    IsDeleted = user.IsDeleted
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving user {id}");
                return StatusCode(500, new { message = "Greška pri preuzimanju korisnika" });
            }
        }

       
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id && !u.IsDeleted);
                if (user == null)
                {
                    return NotFound(new { message = "Korisnik nije pronađen" });
                }

                // Soft delete
                user.IsDeleted = true;
                user.IsActive = false;
                user.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                // Cascade delete: obriši i sve putne planove ovog korisnika u TravelService
                await DeleteTravelPlansForUser(id);

                _logger.LogInformation($"Admin deleted user {id} (soft delete)");
                return Ok(new { message = "Korisnik je uspešno obrisan" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting user {id}");
                return StatusCode(500, new { message = "Greška pri brisanju korisnika" });
            }
        }

      
        [HttpPut("{id}/role")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateRole(int id, [FromBody] UpdateRoleDto dto)
        {
            try
            {
                if (!new[] { "User", "Admin" }.Contains(dto.Role))
                {
                    return BadRequest(new { message = "Uloga mora biti 'User' ili 'Admin'" });
                }

                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id && !u.IsDeleted);
                if (user == null)
                {
                    return NotFound(new { message = "Korisnik nije pronađen" });
                }

                user.Role = dto.Role;
                user.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation($"Admin updated role for user {id} to {dto.Role}");
                return Ok(new UserDto
                {
                    Id = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    Role = user.Role,
                    CreatedAt = user.CreatedAt,
                    UpdatedAt = user.UpdatedAt,
                    IsActive = user.IsActive,
                    IsDeleted = user.IsDeleted
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating role for user {id}");
                return StatusCode(500, new { message = "Greška pri ažuriranju uloge" });
            }
        }

       
        [HttpPut("{id}/activate")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ToggleActive(int id, [FromBody] bool isActive)
        {
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id && !u.IsDeleted);
                if (user == null)
                {
                    return NotFound(new { message = "Korisnik nije pronađen" });
                }

                user.IsActive = isActive;
                user.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation($"Admin toggled active status for user {id} to {isActive}");
                return Ok(new UserDto
                {
                    Id = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    Role = user.Role,
                    CreatedAt = user.CreatedAt,
                    UpdatedAt = user.UpdatedAt,
                    IsActive = user.IsActive,
                    IsDeleted = user.IsDeleted
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error toggling active status for user {id}");
                return StatusCode(500, new { message = "Greška pri promenama statusa korisnika" });
            }
        }

       
        [HttpPatch("{id}/status")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateUserStatus(int id, [FromBody] UpdateUserStatusDto dto)
        {
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id && !u.IsDeleted);
                if (user == null)
                {
                    return NotFound(new { message = "Korisnik nije pronađen" });
                }

                user.IsActive = dto.IsActive;
                user.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation($"Admin updated status for user {id} to active={dto.IsActive}");
                return Ok(new UserDto
                {
                    Id = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    Role = user.Role,
                    CreatedAt = user.CreatedAt,
                    UpdatedAt = user.UpdatedAt,
                    IsActive = user.IsActive,
                    IsDeleted = user.IsDeleted
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating user status for user {id}");
                return StatusCode(500, new { message = "Greška pri promeni statusa korisnika" });
            }
        }
    }
}