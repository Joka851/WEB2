using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using FinanceService.Data;
using FinanceService.DTOs;
using FinanceService.Models;

namespace FinanceService.Controllers
{
    [ApiController]
    [Route("api/travel-plans/{travelPlanId}/expenses")]
    [Authorize]
    public class ExpensesController : ControllerBase
    {
        private readonly FinanceDbContext _context;
        private readonly ILogger<ExpensesController> _logger;

        public ExpensesController(FinanceDbContext context, ILogger<ExpensesController> logger)
        {
            _context = context;
            _logger = logger;
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            return int.Parse(userIdClaim?.Value ?? "0");
        }

        private string GetCurrentUserRole()
        {
            return User.FindFirst(ClaimTypes.Role)?.Value ?? "User";
        }

        /// <summary>
        /// Get all expenses for a travel plan
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll(int travelPlanId)
        {
            try
            {
                // TODO: Add authorization check with TravelService
                var expenses = await _context.Expenses
                    .Where(e => e.TravelPlanId == travelPlanId && !e.IsDeleted)
                    .OrderByDescending(e => e.Date)
                    .Select(e => new ExpenseDto
                    {
                        Id = e.Id,
                        TravelPlanId = e.TravelPlanId,
                        Name = e.Name,
                        Category = e.Category,
                        Amount = e.Amount,
                        Date = e.Date,
                        Description = e.Description,
                        CreatedAt = e.CreatedAt,
                        UpdatedAt = e.UpdatedAt
                    }).ToListAsync();

                _logger.LogInformation($"Retrieved expenses for plan {travelPlanId}");
                return Ok(expenses);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving expenses for plan {travelPlanId}");
                return StatusCode(500, new { message = "Greška pri preuzimanju troškova" });
            }
        }

        /// <summary>
        /// Get expense by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int travelPlanId, int id)
        {
            try
            {
                var expense = await _context.Expenses
                    .FirstOrDefaultAsync(e => e.Id == id && e.TravelPlanId == travelPlanId && !e.IsDeleted);

                if (expense == null)
                {
                    return NotFound(new { message = "Trošak nije pronađen" });
                }

                return Ok(new ExpenseDto
                {
                    Id = expense.Id,
                    TravelPlanId = expense.TravelPlanId,
                    Name = expense.Name,
                    Category = expense.Category,
                    Amount = expense.Amount,
                    Date = expense.Date,
                    Description = expense.Description,
                    CreatedAt = expense.CreatedAt,
                    UpdatedAt = expense.UpdatedAt
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving expense {id}");
                return StatusCode(500, new { message = "Greška pri preuzimanju troška" });
            }
        }

        /// <summary>
        /// Get expense summary with budget analysis
        /// </summary>
        [HttpGet("summary")]
        public async Task<IActionResult> GetSummary(int travelPlanId, [FromQuery] decimal plannedBudget)
        {
            try
            {
                var expenses = await _context.Expenses
                    .Where(e => e.TravelPlanId == travelPlanId && !e.IsDeleted)
                    .ToListAsync();

                var totalExpenses = expenses.Sum(e => e.Amount);
                var remainingBudget = plannedBudget - totalExpenses;
                var budgetUtilization = plannedBudget > 0 ? (totalExpenses / plannedBudget) * 100 : 0;

                var expensesByCategory = expenses
                    .GroupBy(e => e.Category)
                    .ToDictionary(g => g.Key, g => g.Sum(e => e.Amount));

                var summary = new ExpenseSummaryDto
                {
                    TravelPlanId = travelPlanId,
                    PlannedBudget = plannedBudget,
                    TotalExpenses = totalExpenses,
                    RemainingBudget = remainingBudget,
                    ExpensesByCategory = expensesByCategory,
                    BudgetUtilization = (decimal)budgetUtilization
                };

                _logger.LogInformation($"Retrieved expense summary for plan {travelPlanId}");
                return Ok(summary);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving expense summary for plan {travelPlanId}");
                return StatusCode(500, new { message = "Greška pri preuzimanju rezimea troškova" });
            }
        }

        /// <summary>
        /// Get expenses by category
        /// </summary>
        [HttpGet("by-category/{category}")]
        public async Task<IActionResult> GetByCategory(int travelPlanId, string category)
        {
            try
            {
                if (!Expense.IsValidCategory(category))
                {
                    return BadRequest(new { message = "Nevažeća kategorija" });
                }

                var expenses = await _context.Expenses
                    .Where(e => e.TravelPlanId == travelPlanId && e.Category == category && !e.IsDeleted)
                    .OrderByDescending(e => e.Date)
                    .Select(e => new ExpenseDto
                    {
                        Id = e.Id,
                        TravelPlanId = e.TravelPlanId,
                        Name = e.Name,
                        Category = e.Category,
                        Amount = e.Amount,
                        Date = e.Date,
                        Description = e.Description,
                        CreatedAt = e.CreatedAt,
                        UpdatedAt = e.UpdatedAt
                    }).ToListAsync();

                return Ok(expenses);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving expenses by category");
                return StatusCode(500, new { message = "Greška pri preuzimanju troškova po kategoriji" });
            }
        }

        /// <summary>
        /// Get expenses by date range
        /// </summary>
        [HttpGet("by-date-range")]
        public async Task<IActionResult> GetByDateRange(int travelPlanId, [FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            try
            {
                var expenses = await _context.Expenses
                    .Where(e => e.TravelPlanId == travelPlanId &&
                           e.Date >= startDate &&
                           e.Date <= endDate &&
                           !e.IsDeleted)
                    .OrderBy(e => e.Date)
                    .Select(e => new ExpenseDto
                    {
                        Id = e.Id,
                        TravelPlanId = e.TravelPlanId,
                        Name = e.Name,
                        Category = e.Category,
                        Amount = e.Amount,
                        Date = e.Date,
                        Description = e.Description,
                        CreatedAt = e.CreatedAt,
                        UpdatedAt = e.UpdatedAt
                    }).ToListAsync();

                return Ok(expenses);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving expenses by date range");
                return StatusCode(500, new { message = "Greška pri preuzimanju troškova po datumima" });
            }
        }

        /// <summary>
        /// Create a new expense
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Create(int travelPlanId, [FromBody] CreateExpenseDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Validate category
                if (!Expense.IsValidCategory(dto.Category))
                {
                    return BadRequest(new { message = "Nevažeća kategorija troška. Dozvoljene vrednosti: Transport, Accommodation, Food, Activities, Shopping, Insurance, Other" });
                }

                // Validate amount
                if (dto.Amount < 0)
                {
                    return BadRequest(new { message = "Iznos ne može biti negativan." });
                }

                var expense = new Expense
                {
                    TravelPlanId = travelPlanId,
                    Name = dto.Name,
                    Category = dto.Category,
                    Amount = dto.Amount,
                    Date = dto.Date,
                    Description = dto.Description,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.Expenses.Add(expense);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Expense {expense.Id} created for plan {travelPlanId}");

                return CreatedAtAction(nameof(GetById), new { travelPlanId, id = expense.Id }, new ExpenseDto
                {
                    Id = expense.Id,
                    TravelPlanId = expense.TravelPlanId,
                    Name = expense.Name,
                    Category = expense.Category,
                    Amount = expense.Amount,
                    Date = expense.Date,
                    Description = expense.Description,
                    CreatedAt = expense.CreatedAt,
                    UpdatedAt = expense.UpdatedAt
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating expense");
                return StatusCode(500, new { message = "Greška pri kreiranju troška" });
            }
        }

        /// <summary>
        /// Update expense
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int travelPlanId, int id, [FromBody] UpdateExpenseDto dto)
        {
            try
            {
                var expense = await _context.Expenses
                    .FirstOrDefaultAsync(e => e.Id == id && e.TravelPlanId == travelPlanId && !e.IsDeleted);

                if (expense == null)
                {
                    return NotFound(new { message = "Trošak nije pronađen" });
                }

                // Update fields if provided
                if (!string.IsNullOrEmpty(dto.Name))
                    expense.Name = dto.Name;

                if (!string.IsNullOrEmpty(dto.Category))
                {
                    if (!Expense.IsValidCategory(dto.Category))
                    {
                        return BadRequest(new { message = "Nevažeća kategorija troška" });
                    }
                    expense.Category = dto.Category;
                }

                if (dto.Amount.HasValue)
                {
                    if (dto.Amount.Value < 0)
                    {
                        return BadRequest(new { message = "Iznos ne može biti negativan." });
                    }
                    expense.Amount = dto.Amount.Value;
                }

                if (dto.Date.HasValue)
                    expense.Date = dto.Date.Value;

                if (!string.IsNullOrEmpty(dto.Description))
                    expense.Description = dto.Description;

                expense.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Expense {id} updated");

                return Ok(new ExpenseDto
                {
                    Id = expense.Id,
                    TravelPlanId = expense.TravelPlanId,
                    Name = expense.Name,
                    Category = expense.Category,
                    Amount = expense.Amount,
                    Date = expense.Date,
                    Description = expense.Description,
                    CreatedAt = expense.CreatedAt,
                    UpdatedAt = expense.UpdatedAt
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating expense {id}");
                return StatusCode(500, new { message = "Greška pri ažuriranju troška" });
            }
        }

        /// <summary>
        /// Delete expense (soft delete)
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int travelPlanId, int id)
        {
            try
            {
                var expense = await _context.Expenses
                    .FirstOrDefaultAsync(e => e.Id == id && e.TravelPlanId == travelPlanId && !e.IsDeleted);

                if (expense == null)
                {
                    return NotFound(new { message = "Trošak nije pronađen" });
                }

                // Soft delete
                expense.IsDeleted = true;
                expense.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation($"Expense {id} deleted (soft delete)");

                return Ok(new { message = "Trošak je uspešno obrisan" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting expense {id}");
                return StatusCode(500, new { message = "Greška pri brisanju troška" });
            }
        }
    }
}
