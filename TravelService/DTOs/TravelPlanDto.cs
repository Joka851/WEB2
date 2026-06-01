using System.ComponentModel.DataAnnotations;

namespace TravelService.DTOs
{
    public class TravelPlanDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal Budget { get; set; }
        public string Notes { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class CreateTravelPlanDto
    {
        [Required(ErrorMessage = "Naziv je obavezan")]
        [StringLength(255, MinimumLength = 3, ErrorMessage = "Naziv mora sadržati između 3 i 255 karaktera")]
        public string Name { get; set; } = string.Empty;

        [StringLength(1000, ErrorMessage = "Opis može biti maksimalno 1000 karaktera")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Početni datum je obavezan")]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "Krajnji datum je obavezan")]
        public DateTime EndDate { get; set; }

        [Required(ErrorMessage = "Budžet je obavezan")]
        [Range(0, double.MaxValue, ErrorMessage = "Budžet ne može biti negativan")]
        public decimal Budget { get; set; }

        [StringLength(2000, ErrorMessage = "Napomene mogu biti maksimalno 2000 karaktera")]
        public string Notes { get; set; } = string.Empty;

        [Required(ErrorMessage = "ID korisnika je obavezan")]
        public int UserId { get; set; }
    }

    public class UpdateTravelPlanDto
    {
        [StringLength(255, MinimumLength = 3, ErrorMessage = "Naziv mora sadržati između 3 i 255 karaktera")]
        public string? Name { get; set; }

        [StringLength(1000, ErrorMessage = "Opis može biti maksimalno 1000 karaktera")]
        public string? Description { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Budžet ne može biti negativan")]
        public decimal? Budget { get; set; }

        [StringLength(2000, ErrorMessage = "Napomene mogu biti maksimalno 2000 karaktera")]
        public string? Notes { get; set; }
    }

    public class TravelPlanDetailDto : TravelPlanDto
    {
        public decimal TotalEstimatedCosts { get; set; }
        public decimal RemainingBudget { get; set; }
        public int DestinationCount { get; set; }
        public int ActivityCount { get; set; }
        public int ChecklistItemCount { get; set; }
    }
}
