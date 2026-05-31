using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FinanceService.Data;
using FinanceService.DTOs;
using FinanceService.Models;

namespace FinanceService.Controllers
{
    [ApiController]
    [Route("api/travel-plans/{travelPlanId}/expenses")]
    public class ExpensesController : ControllerBase
    {
        private readonly FinanceDbContext _context;

        public ExpensesController(FinanceDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(int travelPlanId)
        {
            var expenses = await _context.Expenses
                .Where(e => e.TravelPlanId == travelPlanId)
                .Select(e => new ExpenseDto
                {
                    Id = e.Id,
                    TravelPlanId = e.TravelPlanId,
                    Name = e.Name,
                    Category = e.Category,
                    Amount = e.Amount,
                    Date = e.Date,
                    Description = e.Description
                }).ToListAsync();

            return Ok(expenses);
        }

        [HttpGet("summary")]
        public async Task<IActionResult> GetSummary(int travelPlanId)
        {
            var expenses = await _context.Expenses
                .Where(e => e.TravelPlanId == travelPlanId)
                .ToListAsync();

            var summary = new
            {
                TravelPlanId = travelPlanId,
                TotalExpenses = expenses.Sum(e => e.Amount),
                ExpensesByCategory = expenses
                    .GroupBy(e => e.Category)
                    .Select(g => new { Category = g.Key, Total = g.Sum(e => e.Amount) })
            };

            return Ok(summary);
        }

        [HttpPost]
        public async Task<IActionResult> Create(int travelPlanId, CreateExpenseDto dto)
        {
            if (dto.Amount < 0)
                return BadRequest(new { message = "Amount cannot be negative." });

            var expense = new Expense
            {
                TravelPlanId = travelPlanId,
                Name = dto.Name,
                Category = dto.Category,
                Amount = dto.Amount,
                Date = dto.Date,
                Description = dto.Description
            };

            _context.Expenses.Add(expense);
            await _context.SaveChangesAsync();

            return Ok(new ExpenseDto
            {
                Id = expense.Id,
                TravelPlanId = expense.TravelPlanId,
                Name = expense.Name,
                Category = expense.Category,
                Amount = expense.Amount,
                Date = expense.Date,
                Description = expense.Description
            });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int travelPlanId, int id, CreateExpenseDto dto)
        {
            var expense = await _context.Expenses
                .FirstOrDefaultAsync(e => e.Id == id && e.TravelPlanId == travelPlanId);
            if (expense == null) return NotFound();

            if (dto.Amount < 0)
                return BadRequest(new { message = "Amount cannot be negative." });

            expense.Name = dto.Name;
            expense.Category = dto.Category;
            expense.Amount = dto.Amount;
            expense.Date = dto.Date;
            expense.Description = dto.Description;

            await _context.SaveChangesAsync();

            return Ok(new ExpenseDto
            {
                Id = expense.Id,
                TravelPlanId = expense.TravelPlanId,
                Name = expense.Name,
                Category = expense.Category,
                Amount = expense.Amount,
                Date = expense.Date,
                Description = expense.Description
            });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int travelPlanId, int id)
        {
            var expense = await _context.Expenses
                .FirstOrDefaultAsync(e => e.Id == id && e.TravelPlanId == travelPlanId);
            if (expense == null) return NotFound();

            _context.Expenses.Remove(expense);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}