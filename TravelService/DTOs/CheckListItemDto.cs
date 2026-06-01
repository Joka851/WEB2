using System.ComponentModel.DataAnnotations;

namespace TravelService.DTOs
{
    public class ChecklistItemDto
    {
        public int Id { get; set; }
        public int TravelPlanId { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool IsCompleted { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class CreateChecklistItemDto
    {
        [Required(ErrorMessage = "Naziv stavke je obavezan")]
        [StringLength(255, MinimumLength = 3, ErrorMessage = "Naziv mora sadržati između 3 i 255 karaktera")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Status je obavezan")]
        public bool IsCompleted { get; set; } = false;
    }

    public class UpdateChecklistItemDto
    {
        [StringLength(255, MinimumLength = 3, ErrorMessage = "Naziv mora sadržati između 3 i 255 karaktera")]
        public string? Name { get; set; }

        public bool? IsCompleted { get; set; }
    }
}
