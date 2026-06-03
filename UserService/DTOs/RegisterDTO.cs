<<<<<<< HEAD
﻿using System.ComponentModel.DataAnnotations;
=======
using System.ComponentModel.DataAnnotations;
>>>>>>> 33c3fde4fc508e3f120288c3e2683a8b615d44dc

namespace UserService.DTOs
{
    public class RegisterDto
    {
        [Required(ErrorMessage = "Ime je obavezno")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Ime mora sadržati između 2 i 100 karaktera")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Prezime je obavezno")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Prezime mora sadržati između 2 i 100 karaktera")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email je obavezan")]
        [EmailAddress(ErrorMessage = "Email nije validan")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Lozinka je obavezna")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Lozinka mora sadržati između 6 i 100 karaktera")]
        public string Password { get; set; } = string.Empty;
    }
}
