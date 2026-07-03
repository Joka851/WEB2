using System.ComponentModel.DataAnnotations;

namespace UserService.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Ime je obavezno")]
        [StringLength(100, MinimumLength = 2,
            ErrorMessage = "Ime mora sadržati između 2 i 100 karaktera")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Prezime je obavezno")]
        [StringLength(100, MinimumLength = 2,
            ErrorMessage = "Prezime mora sadržati između 2 i 100 karaktera")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email je obavezan")]
        [EmailAddress(ErrorMessage = "Email nije validan")]
        [StringLength(255, ErrorMessage = "Email može biti maksimalno 255 karaktera")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Lozinka je obavezna")]
        [StringLength(500, ErrorMessage = "Hash lozinke može biti maksimalno 500 karaktera")]
        public string PasswordHash { get; set; } = string.Empty;

        [Required(ErrorMessage = "Uloga je obavezna")]
        [StringLength(50, ErrorMessage = "Uloga može biti maksimalno 50 karaktera")]
        public string Role { get; set; } = "User"; // User ili Admin

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        public bool IsActive { get; set; } = true;

        [Required]
        public bool IsDeleted { get; set; } = false;

       
    }
}
