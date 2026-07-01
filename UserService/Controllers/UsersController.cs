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

        public UsersController(UserDbContext context, ILogger<UsersController> logger)
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

        /// <summary>
        /// Get all users - Only Admin can do this
        /// </summary>
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

        /// <summary>
        /// Get user by ID - User can get own profile, Admin can get any
        /// </summary>
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

        /// <summary>
        /// Update user profile - User can update own, Admin can update any
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateUserDto dto)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var currentRole = GetCurrentUserRole();

                // User can only update their own profile, except Admin
                if (currentUserId != id && currentRole != "Admin")
                {
                    _logger.LogWarning($"User {currentUserId} attempted to update profile of user {id}");
                    return Forbid();
                }

                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id && !u.IsDeleted);
                if (user == null)
                {
                    return NotFound(new { message = "Korisnik nije pronađen" });
                }

                // Check if new email already exists
                if (!string.IsNullOrEmpty(dto.Email) && dto.Email != user.Email)
                {
                    if (await _context.Users.AnyAsync(u => u.Email == dto.Email && !u.IsDeleted))
                    {
                        return BadRequest(new { message = "Email je već u upotrebi" });
                    }
                    user.Email = dto.Email;
                }

                if (!string.IsNullOrEmpty(dto.FirstName))
                    user.FirstName = dto.FirstName;

                if (!string.IsNullOrEmpty(dto.LastName))
                    user.LastName = dto.LastName;

                user.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation($"User {id} updated successfully");
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
                _logger.LogError(ex, $"Error updating user {id}");
                return StatusCode(500, new { message = "Greška pri ažuriranju korisnika" });
            }
        }

        /// <summary>
        /// Change password - User can change own, Admin can change any
        /// </summary>
        [HttpPost("{id}/change-password")]
        public async Task<IActionResult> ChangePassword(int id, [FromBody] ChangePasswordDto dto)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var currentRole = GetCurrentUserRole();

                // User can only change their own password, except Admin
                if (currentUserId != id && currentRole != "Admin")
                {
                    _logger.LogWarning($"User {currentUserId} attempted to change password of user {id}");
                    return Forbid();
                }

                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id && !u.IsDeleted);
                if (user == null)
                {
                    return NotFound(new { message = "Korisnik nije pronađen" });
                }

                // Verify old password
                if (!BCrypt.Net.BCrypt.Verify(dto.OldPassword, user.PasswordHash))
                {
                    _logger.LogWarning($"Invalid old password attempt for user {id}");
                    return BadRequest(new { message = "Stara lozinka nije tačna" });
                }

                // Hash new password
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
                user.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation($"User {id} changed password successfully");
                return Ok(new { message = "Lozinka je uspešno promenjena" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error changing password for user {id}");
                return StatusCode(500, new { message = "Greška pri promeni lozinke" });
            }
        }

        /// <summary>
        /// Delete user - Only Admin can delete users (soft delete)
        /// </summary>
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

                _logger.LogInformation($"Admin deleted user {id} (soft delete)");
                return Ok(new { message = "Korisnik je uspešno obrisan" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting user {id}");
                return StatusCode(500, new { message = "Greška pri brisanju korisnika" });
            }
        }

       

        /// <summary>
        /// Activate/Deactivate user - Only Admin can do this
        /// </summary>
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

       
        /// <summary>
        /// Update user status (activate/deactivate) - Only Admin can do this
        /// Ovo je endpoint koji frontend očekuje (PATCH /api/users/{id}/status)
        /// </summary>
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