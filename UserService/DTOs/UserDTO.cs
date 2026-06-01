using System.ComponentModel.DataAnnotations;

namespace UserService.DTOs
{
    public class UserDto
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Ime je obavezno")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Prezime je obavezno")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email je obavezan")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Uloga je obavezna")]
        public string Role { get; set; } = "User";

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        [Required]
        public bool IsActive { get; set; } = true;
    }

    public class UpdateUserDto
    {
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Ime mora sadržati između 2 i 100 karaktera")]
        public string? FirstName { get; set; }

        [StringLength(100, MinimumLength = 2, ErrorMessage = "Prezime mora sadržati između 2 i 100 karaktera")]
        public string? LastName { get; set; }

        [EmailAddress(ErrorMessage = "Email nije validan")]
        public string? Email { get; set; }
    }

    public class UpdateRoleDto
    {
        [Required(ErrorMessage = "Uloga je obavezna")]
        [RegularExpression("^(User|Admin)$", ErrorMessage = "Uloga mora biti 'User' ili 'Admin'")]
        public string Role { get; set; } = string.Empty;
    }

    public class ChangePasswordDto
    {
        [Required(ErrorMessage = "Stara lozinka je obavezna")]
        public string OldPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Nova lozinka je obavezna")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Lozinka mora sadržati između 6 i 100 karaktera")]
        public string NewPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Potvrda lozinke je obavezna")]
        [Compare("NewPassword", ErrorMessage = "Lozinke se ne poklapaju")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
