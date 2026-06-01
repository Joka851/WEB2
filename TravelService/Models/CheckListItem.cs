using System.ComponentModel.DataAnnotations;

namespace TravelService.Models
{
    public class ChecklistItem
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "ID putnog plana je obavezan")]
        public int TravelPlanId { get; set; }

        [Required(ErrorMessage = "Naziv stavke je obavezan")]
        [StringLength(255, MinimumLength = 3,
            ErrorMessage = "Naziv mora sadržati između 3 i 255 karaktera")]
        public string Name { get; set; } = string.Empty;

        [Required]
        public bool IsCompleted { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        public bool IsDeleted { get; set; } = false;

        // Navigation property
        [Required]
        public TravelPlan TravelPlan { get; set; } = null!;

        // Validation
        public bool IsValid()
        {
            return !string.IsNullOrWhiteSpace(Name) && TravelPlanId > 0;
        }
    }
}
