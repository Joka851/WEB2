using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UserService.Data;
using UserService.DTOs;
using UserService.Services;

namespace UserService.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly UserDbContext _context;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, UserDbContext context, ILogger<AuthController> logger)
        {
            _authService = authService;
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Register a new user
        /// </summary>
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            try
            {
                // Validate input
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Check if email already exists
                if (await _context.Users.AnyAsync(u => u.Email == dto.Email && !u.IsDeleted))
                {
                    _logger.LogWarning($"Registration attempt with existing email: {dto.Email}");
                    return BadRequest(new { message = "Email je već u upotrebi." });
                }

                // Register user
                var result = await _authService.Register(dto);
                if (result == null)
                {
                    _logger.LogWarning($"Registration failed for email: {dto.Email}");
                    return BadRequest(new { message = "Neuspešna registracija." });
                }

                _logger.LogInformation($"User registered successfully: {dto.Email}");
                return CreatedAtAction(nameof(Register), new { email = result.Email }, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during registration");
                return StatusCode(500, new { message = "Greška pri registraciji korisnika." });
            }
        }

        /// <summary>
        /// Login user and return JWT token
        /// </summary>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            try
            {
                // Validate input
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Attempt login
                var token = await _authService.Login(dto);
                if (token == null)
                {
                    _logger.LogWarning($"Failed login attempt for email: {dto.Email}");
                    return Unauthorized(new { message = "Nevažeći email ili lozinka." });
                }

                _logger.LogInformation($"User logged in successfully: {dto.Email}");
                return Ok(new { token, message = "Uspešna prijava" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login");
                return StatusCode(500, new { message = "Greška pri prijavi korisnika." });
            }
        }
    }
}
