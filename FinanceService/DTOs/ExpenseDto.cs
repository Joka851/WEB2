using System.ComponentModel.DataAnnotations;

namespace FinanceService.DTOs
{
    public class ExpenseDto
    {
        public int Id { get; set; }
        public int TravelPlanId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime Date { get; set; }
        public string Description { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class CreateExpenseDto
    {
        [Required(ErrorMessage = "Naziv troška je obavezan")]
        [StringLength(255, MinimumLength = 3, ErrorMessage = "Naziv mora sadržati između 3 i 255 karaktera")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Kategorija je obavezna")]
        [RegularExpression("^(Transport|Accommodation|Food|Activities|Shopping|Insurance|Other)$",
            ErrorMessage = "Kategorija mora biti jedna od: Transport, Accommodation, Food, Activities, Shopping, Insurance, Other")]
        public string Category { get; set; } = "Other";

        [Required(ErrorMessage = "Iznos je obavezan")]
        [Range(0, double.MaxValue, ErrorMessage = "Iznos ne može biti negativan")]
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "Datum je obavezan")]
        public DateTime Date { get; set; }

        [StringLength(1000, ErrorMessage = "Opis može biti maksimalno 1000 karaktera")]
        public string Description { get; set; } = string.Empty;
    }

    public class UpdateExpenseDto
    {
        [StringLength(255, MinimumLength = 3, ErrorMessage = "Naziv mora sadržati između 3 i 255 karaktera")]
        public string? Name { get; set; }

        [RegularExpression("^(Transport|Accommodation|Food|Activities|Shopping|Insurance|Other)$",
            ErrorMessage = "Kategorija mora biti jedna od: Transport, Accommodation, Food, Activities, Shopping, Insurance, Other")]
        public string? Category { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Iznos ne može biti negativan")]
        public decimal? Amount { get; set; }

        public DateTime? Date { get; set; }

        [StringLength(1000, ErrorMessage = "Opis može biti maksimalno 1000 karaktera")]
        public string? Description { get; set; }
    }

    public class ExpenseSummaryDto
    {
        public int TravelPlanId { get; set; }
        public decimal PlannedBudget { get; set; }
        public decimal TotalExpenses { get; set; }
        public decimal RemainingBudget { get; set; }
        public Dictionary<string, decimal> ExpensesByCategory { get; set; } = new();
        public decimal BudgetUtilization { get; set; } // Percentage
    }

  
}
