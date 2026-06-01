using System.ComponentModel.DataAnnotations;

namespace FinanceService.Models
{
    public class Expense
    {
        public const string CATEGORY_TRANSPORT = "Transport";
        public const string CATEGORY_ACCOMMODATION = "Accommodation";
        public const string CATEGORY_FOOD = "Food";
        public const string CATEGORY_ACTIVITIES = "Activities";
        public const string CATEGORY_SHOPPING = "Shopping";
        public const string CATEGORY_INSURANCE = "Insurance";
        public const string CATEGORY_OTHER = "Other";

        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "ID putnog plana je obavezan")]
        public int TravelPlanId { get; set; }

        [Required(ErrorMessage = "Naziv troška je obavezan")]
        [StringLength(255, MinimumLength = 3,
            ErrorMessage = "Naziv mora sadržati između 3 i 255 karaktera")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Kategorija je obavezna")]
        [StringLength(100, ErrorMessage = "Kategorija može biti maksimalno 100 karaktera")]
        public string Category { get; set; } = CATEGORY_OTHER;

        [Required(ErrorMessage = "Iznos je obavezan")]
        [Range(0, double.MaxValue, ErrorMessage = "Iznos ne može biti negativan")]
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "Datum troška je obavezan")]
        public DateTime Date { get; set; }

        [StringLength(1000, ErrorMessage = "Opis može biti maksimalno 1000 karaktera")]
        public string Description { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        public bool IsDeleted { get; set; } = false;

        // Validation
        public bool IsValid()
        {
            var validCategories = new[]
            {
                CATEGORY_TRANSPORT,
                CATEGORY_ACCOMMODATION,
                CATEGORY_FOOD,
                CATEGORY_ACTIVITIES,
                CATEGORY_SHOPPING,
                CATEGORY_INSURANCE,
                CATEGORY_OTHER
            };

            return !string.IsNullOrWhiteSpace(Name) &&
                   !string.IsNullOrWhiteSpace(Category) &&
                   validCategories.Contains(Category) &&
                   Amount >= 0 &&
                   TravelPlanId > 0;
        }

        // Check if category is valid
        public static bool IsValidCategory(string category)
        {
            var validCategories = new[]
            {
                CATEGORY_TRANSPORT,
                CATEGORY_ACCOMMODATION,
                CATEGORY_FOOD,
                CATEGORY_ACTIVITIES,
                CATEGORY_SHOPPING,
                CATEGORY_INSURANCE,
                CATEGORY_OTHER
            };
            return validCategories.Contains(category);
        }

        // Get all valid categories
        public static string[] GetValidCategories()
        {
            return new[]
            {
                CATEGORY_TRANSPORT,
                CATEGORY_ACCOMMODATION,
                CATEGORY_FOOD,
                CATEGORY_ACTIVITIES,
                CATEGORY_SHOPPING,
                CATEGORY_INSURANCE,
                CATEGORY_OTHER
            };
        }
    }
}
