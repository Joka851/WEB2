using System.ComponentModel.DataAnnotations;

namespace TravelService.Models
{
    public class TravelPlan
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "ID korisnika je obavezan")]
        public int UserId { get; set; }

        [Required(ErrorMessage = "Naziv je obavezan")]
        [StringLength(255, MinimumLength = 3,
            ErrorMessage = "Naziv mora sadržati između 3 i 255 karaktera")]
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

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        public bool IsDeleted { get; set; } = false;

        // Navigation properties
        public ICollection<Destination> Destinations { get; set; } = new List<Destination>();
        public ICollection<Activity> Activities { get; set; } = new List<Activity>();
        public ICollection<ChecklistItem> ChecklistItems { get; set; } = new List<ChecklistItem>();
        public ICollection<ShareToken> ShareTokens { get; set; } = new List<ShareToken>();

       

        // Calculate total expenses from activities
        public decimal GetTotalEstimatedCosts()
        {
            return Activities.Where(a => !a.IsDeleted).Sum(a => a.EstimatedCost);
        }

        // Get remaining budget
        public decimal GetRemainingBudget()
        {
            return Budget - GetTotalEstimatedCosts();
        }
    }
}
