using System.ComponentModel.DataAnnotations;

namespace TravelService.DTOs
{
    public class DestinationDto
    {
        public int Id { get; set; }
        public int TravelPlanId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public DateTime ArrivalDate { get; set; }
        public DateTime DepartureDate { get; set; }
        public string Description { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class CreateDestinationDto
    {
        [Required(ErrorMessage = "Naziv destinacije je obavezan")]
        [StringLength(255, MinimumLength = 3, ErrorMessage = "Naziv mora sadržati između 3 i 255 karaktera")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Lokacija je obavezna")]
        [StringLength(255, MinimumLength = 3, ErrorMessage = "Lokacija mora sadržati između 3 i 255 karaktera")]
        public string Location { get; set; } = string.Empty;

        [Required(ErrorMessage = "Datum dolaska je obavezan")]
        public DateTime ArrivalDate { get; set; }

        [Required(ErrorMessage = "Datum odlaska je obavezan")]
        public DateTime DepartureDate { get; set; }

        [StringLength(1000, ErrorMessage = "Opis može biti maksimalno 1000 karaktera")]
        public string Description { get; set; } = string.Empty;
    }

    public class UpdateDestinationDto
    {
        [StringLength(255, MinimumLength = 3, ErrorMessage = "Naziv mora sadržati između 3 i 255 karaktera")]
        public string? Name { get; set; }

        [StringLength(255, MinimumLength = 3, ErrorMessage = "Lokacija mora sadržati između 3 i 255 karaktera")]
        public string? Location { get; set; }

        public DateTime? ArrivalDate { get; set; }

        public DateTime? DepartureDate { get; set; }

        [StringLength(1000, ErrorMessage = "Opis može biti maksimalno 1000 karaktera")]
        public string? Description { get; set; }
    }
}
