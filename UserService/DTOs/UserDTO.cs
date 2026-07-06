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

        [Required]
        public bool IsDeleted { get; set; } = false;
    }

    public class UpdateRoleDto
    {
        [Required(ErrorMessage = "Uloga je obavezna")]
        [RegularExpression("^(User|Admin)$", ErrorMessage = "Uloga mora biti 'User' ili 'Admin'")]
        public string Role { get; set; } = string.Empty;
    }
}