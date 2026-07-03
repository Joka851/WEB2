using System.ComponentModel.DataAnnotations;

namespace TravelService.Models
{
    public class Activity
    {
        public const string STATUS_PLANNED = "Planned";
        public const string STATUS_RESERVED = "Reserved";
        public const string STATUS_COMPLETED = "Completed";
        public const string STATUS_CANCELLED = "Cancelled";

        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "ID putnog plana je obavezan")]
        public int TravelPlanId { get; set; }

        [Required(ErrorMessage = "Naziv aktivnosti je obavezan")]
        [StringLength(255, MinimumLength = 3,
            ErrorMessage = "Naziv mora sadržati između 3 i 255 karaktera")]
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
        [StringLength(50, ErrorMessage = "Status može biti maksimalno 50 karaktera")]
        public string Status { get; set; } = STATUS_PLANNED;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        public bool IsDeleted { get; set; } = false;

        // Navigation property
        [Required]
        public TravelPlan TravelPlan { get; set; } = null!;

       

        // Check if status is valid
        public static bool IsValidStatus(string status)
        {
            var validStatuses = new[] { STATUS_PLANNED, STATUS_RESERVED, STATUS_COMPLETED, STATUS_CANCELLED };
            return validStatuses.Contains(status);
        }
    }
}
