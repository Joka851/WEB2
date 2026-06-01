using System.ComponentModel.DataAnnotations;

namespace UserService.DTOs
{
    public class LoginDto
    {
        [Required(ErrorMessage = "Email je obavezan")]
        [EmailAddress(ErrorMessage = "Email nije validan")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Lozinka je obavezna")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Lozinka mora sadržati između 6 i 100 karaktera")]
        public string Password { get; set; } = string.Empty;
    }
}
