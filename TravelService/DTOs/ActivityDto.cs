using System.ComponentModel.DataAnnotations;

namespace TravelService.DTOs
{
    public class ActivityDto
    {
        public int Id { get; set; }
        public int TravelPlanId { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public string Time { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal EstimatedCost { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class CreateActivityDto
    {
        [Required(ErrorMessage = "Naziv aktivnosti je obavezan")]
        [StringLength(255, MinimumLength = 3, ErrorMessage = "Naziv mora sadržati između 3 i 255 karaktera")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Datum aktivnosti je obavezan")]
        public DateTime Date { get; set; }

        [StringLength(50, ErrorMessage = "Vreme može biti maksimalno 50 karaktera")]
        public string Time { get; set; } = string.Empty;

        [StringLength(255, ErrorMessage = "Lokacija može biti maksimalno 255 karaktera")]
        public string Location { get; set; } = string.Empty;

        [StringLength(1000, ErrorMessage = "Opis može biti maksimalno 1000 karaktera")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Procenjen trošak je obavezan")]
        [Range(0, double.MaxValue, ErrorMessage = "Trošak ne može biti negativan")]
        public decimal EstimatedCost { get; set; }

        [Required(ErrorMessage = "Status je obavezan")]
        [RegularExpression("^(Planned|Reserved|Completed|Cancelled)$",
            ErrorMessage = "Status mora biti jedan od: Planned, Reserved, Completed, Cancelled")]
        public string Status { get; set; } = "Planned";
    }

    public class UpdateActivityDto
    {
        [StringLength(255, MinimumLength = 3, ErrorMessage = "Naziv mora sadržati između 3 i 255 karaktera")]
        public string? Name { get; set; }

        public DateTime? Date { get; set; }

        [StringLength(50, ErrorMessage = "Vreme može biti maksimalno 50 karaktera")]
        public string? Time { get; set; }

        [StringLength(255, ErrorMessage = "Lokacija može biti maksimalno 255 karaktera")]
        public string? Location { get; set; }

        [StringLength(1000, ErrorMessage = "Opis može biti maksimalno 1000 karaktera")]
        public string? Description { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Trošak ne može biti negativan")]
        public decimal? EstimatedCost { get; set; }

        [RegularExpression("^(Planned|Reserved|Completed|Cancelled)$",
            ErrorMessage = "Status mora biti jedan od: Planned, Reserved, Completed, Cancelled")]
        public string? Status { get; set; }
    }
}
