using System.ComponentModel.DataAnnotations;

namespace TravelService.Models
{
    public class Destination
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "ID putnog plana je obavezan")]
        public int TravelPlanId { get; set; }

        [Required(ErrorMessage = "Naziv destinacije je obavezan")]
        [StringLength(255, MinimumLength = 3,
            ErrorMessage = "Naziv mora sadržati između 3 i 255 karaktera")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Lokacija je obavezna")]
        [StringLength(255, MinimumLength = 3,
            ErrorMessage = "Lokacija mora sadržati između 3 i 255 karaktera")]
        public string Location { get; set; } = string.Empty;

        [Required(ErrorMessage = "Datum dolaska je obavezan")]
        public DateTime ArrivalDate { get; set; }

        [Required(ErrorMessage = "Datum odlaska je obavezan")]
        public DateTime DepartureDate { get; set; }

        [StringLength(1000, ErrorMessage = "Opis može biti maksimalno 1000 karaktera")]
        public string Description { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        public bool IsDeleted { get; set; } = false;

        // Navigation property
        [Required]
        public TravelPlan TravelPlan { get; set; } = null!;

       
    }
}
