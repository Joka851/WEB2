using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net.Http.Json;
using System.Security.Claims;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using TravelService.Data;
using TravelService.Models;

namespace TravelService.Controllers
{
    [ApiController]
    [Route("api/travel-plans/{travelPlanId}/pdf")]
    [Authorize]
    public class PdfController : ControllerBase
    {
        private readonly TravelDbContext _context;
        private readonly ILogger<PdfController> _logger;
        private readonly IHttpClientFactory _httpClientFactory;

        public PdfController(TravelDbContext context, ILogger<PdfController> logger, IHttpClientFactory httpClientFactory)
        {
            _context = context;
            _logger = logger;
            _httpClientFactory = httpClientFactory;
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

       
        private class RemoteExpenseDto
        {
            public int Id { get; set; }
            public string Name { get; set; } = string.Empty;
            public string Category { get; set; } = string.Empty;
            public decimal Amount { get; set; }
            public DateTime Date { get; set; }
        }

       
        private async Task<List<RemoteExpenseDto>> TryGetExpenses(int travelPlanId)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("FinanceService");
                var request = new HttpRequestMessage(HttpMethod.Get, $"/api/travel-plans/{travelPlanId}/expenses");

                if (Request.Headers.TryGetValue("Authorization", out var authHeader))
                {
                    request.Headers.TryAddWithoutValidation("Authorization", authHeader.ToString());
                }

                var response = await client.SendAsync(request);
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("FinanceService je vratio {StatusCode} pri generisanju PDF-a za plan {PlanId}",
                        response.StatusCode, travelPlanId);
                    return new List<RemoteExpenseDto>();
                }

                var expenses = await response.Content.ReadFromJsonAsync<List<RemoteExpenseDto>>();
                return expenses ?? new List<RemoteExpenseDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška pri preuzimanju troškova za PDF izveštaj plana {PlanId}", travelPlanId);
                return new List<RemoteExpenseDto>();
            }
        }

        private static string SanitizeFileName(string name)
        {
            var invalidChars = Path.GetInvalidFileNameChars();
            var cleaned = new string(name.Where(c => !invalidChars.Contains(c)).ToArray());
            return string.IsNullOrWhiteSpace(cleaned) ? "PutniPlan" : cleaned.Replace(' ', '_');
        }

        [HttpGet]
        public async Task<IActionResult> GetPdf(int travelPlanId)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var currentRole = GetCurrentUserRole();

                var plan = await _context.TravelPlans
                    .Include(t => t.Destinations)
                    .Include(t => t.Activities)
                    .Include(t => t.ChecklistItems)
                    .FirstOrDefaultAsync(t => t.Id == travelPlanId && !t.IsDeleted);

                if (plan == null)
                {
                    return NotFound(new { message = "Putni plan nije pronađen" });
                }

                if (plan.UserId != currentUserId && currentRole != "Admin")
                {
                    _logger.LogWarning($"User {currentUserId} attempted unauthorized PDF export of plan {travelPlanId}");
                    return Forbid();
                }

                var destinations = plan.Destinations.Where(d => !d.IsDeleted).OrderBy(d => d.ArrivalDate).ToList();
                var activities = plan.Activities.Where(a => !a.IsDeleted).OrderBy(a => a.Date).ThenBy(a => a.Time).ToList();
                var checklistItems = plan.ChecklistItems.Where(c => !c.IsDeleted).OrderBy(c => c.IsCompleted).ThenBy(c => c.CreatedAt).ToList();
                var expenses = await TryGetExpenses(travelPlanId);

                var totalEstimatedCosts = plan.GetTotalEstimatedCosts();
                var remainingBudgetEstimated = plan.GetRemainingBudget();
                var totalActualExpenses = expenses.Sum(e => e.Amount);
                var remainingBudgetActual = plan.Budget - totalActualExpenses;

                QuestPDF.Settings.License = LicenseType.Community;

                var document = Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4);
                        page.Margin(35);
                        page.DefaultTextStyle(x => x.FontSize(10));

                        page.Header().Column(col =>
                        {
                            col.Item().Text(plan.Name).FontSize(20).Bold();
                            col.Item().PaddingTop(2).Text($"Izveštaj generisan: {DateTime.Now:dd.MM.yyyy HH:mm}").FontSize(8).FontColor(Colors.Grey.Darken1);
                            col.Item().PaddingTop(5).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                        });

                        page.Content().PaddingTop(15).Column(col =>
                        {
                            col.Spacing(12);

                            // ---- Osnovni podaci ----
                            col.Item().Text("Osnovni podaci").FontSize(14).Bold();
                            col.Item().Column(inner =>
                            {
                                inner.Item().Text($"Opis: {plan.Description}");
                                inner.Item().Text($"Period: {plan.StartDate:dd.MM.yyyy} - {plan.EndDate:dd.MM.yyyy}");
                                inner.Item().Text($"Planirani budžet: {plan.Budget:N2}");
                                if (!string.IsNullOrWhiteSpace(plan.Notes))
                                    inner.Item().Text($"Napomene: {plan.Notes}");
                            });

                            // ---- Destinacije ----
                            col.Item().Text($"Destinacije ({destinations.Count})").FontSize(14).Bold();
                            if (destinations.Count == 0)
                            {
                                col.Item().Text("Nema dodatih destinacija.").FontColor(Colors.Grey.Darken1);
                            }
                            else
                            {
                                foreach (var d in destinations)
                                {
                                    col.Item().BorderLeft(2).BorderColor(Colors.Blue.Medium).PaddingLeft(8).Column(inner =>
                                    {
                                        inner.Item().Text($"{d.Name} — {d.Location}").Bold();
                                        inner.Item().Text($"{d.ArrivalDate:dd.MM.yyyy} - {d.DepartureDate:dd.MM.yyyy}").FontSize(9);
                                        if (!string.IsNullOrWhiteSpace(d.Description))
                                            inner.Item().Text(d.Description).FontSize(9);
                                    });
                                }
                            }

                            // ---- Aktivnosti ----
                            col.Item().Text($"Aktivnosti ({activities.Count})").FontSize(14).Bold();
                            if (activities.Count == 0)
                            {
                                col.Item().Text("Nema dodatih aktivnosti.").FontColor(Colors.Grey.Darken1);
                            }
                            else
                            {
                                col.Item().Table(table =>
                                {
                                    table.ColumnsDefinition(columns =>
                                    {
                                        columns.RelativeColumn(3);
                                        columns.RelativeColumn(2);
                                        columns.RelativeColumn(2);
                                        columns.RelativeColumn(2);
                                        columns.RelativeColumn(2);
                                    });

                                    table.Header(header =>
                                    {
                                        header.Cell().Text("Naziv").Bold();
                                        header.Cell().Text("Datum").Bold();
                                        header.Cell().Text("Lokacija").Bold();
                                        header.Cell().Text("Trošak").Bold();
                                        header.Cell().Text("Status").Bold();
                                        header.Cell().ColumnSpan(5).PaddingTop(2).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                                    });

                                    foreach (var a in activities)
                                    {
                                        table.Cell().Text(a.Name);
                                        table.Cell().Text($"{a.Date:dd.MM.yyyy} {a.Time}".Trim());
                                        table.Cell().Text(a.Location ?? "-");
                                        table.Cell().Text($"{a.EstimatedCost:N2}");
                                        table.Cell().Text(a.Status);
                                    }
                                });
                            }

                            // ---- Checklist ----
                            col.Item().Text($"Checklist ({checklistItems.Count(c => c.IsCompleted)}/{checklistItems.Count})").FontSize(14).Bold();
                            if (checklistItems.Count == 0)
                            {
                                col.Item().Text("Nema stavki na checklisti.").FontColor(Colors.Grey.Darken1);
                            }
                            else
                            {
                                foreach (var c in checklistItems)
                                {
                                    col.Item().Text(text =>
                                    {
                                        text.Span(c.IsCompleted ? "[✓] " : "[ ] ").Bold();
                                        text.Span(c.Name);
                                    });
                                }
                            }

                            // ---- Troškovi i budžet ----
                            col.Item().Text("Troškovi i budžet").FontSize(14).Bold();
                            col.Item().Column(inner =>
                            {
                                inner.Item().Text($"Planirani budžet: {plan.Budget:N2}");
                                inner.Item().Text($"Ukupno procenjeni troškovi (iz aktivnosti): {totalEstimatedCosts:N2}");
                                inner.Item().Text($"Preostali budžet (procena): {remainingBudgetEstimated:N2}");

                                if (expenses.Count > 0)
                                {
                                    inner.Item().PaddingTop(5).Text($"Ukupno evidentirani stvarni troškovi: {totalActualExpenses:N2}").Bold();
                                    inner.Item().Text($"Preostali budžet (stvaran): {remainingBudgetActual:N2}").Bold();

                                    inner.Item().PaddingTop(5).Table(table =>
                                    {
                                        table.ColumnsDefinition(columns =>
                                        {
                                            columns.RelativeColumn(3);
                                            columns.RelativeColumn(2);
                                            columns.RelativeColumn(2);
                                            columns.RelativeColumn(2);
                                        });

                                        table.Header(header =>
                                        {
                                            header.Cell().Text("Naziv").Bold();
                                            header.Cell().Text("Kategorija").Bold();
                                            header.Cell().Text("Datum").Bold();
                                            header.Cell().Text("Iznos").Bold();
                                            header.Cell().ColumnSpan(4).PaddingTop(2).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                                        });

                                        foreach (var e in expenses.OrderBy(e => e.Date))
                                        {
                                            table.Cell().Text(e.Name);
                                            table.Cell().Text(e.Category);
                                            table.Cell().Text($"{e.Date:dd.MM.yyyy}");
                                            table.Cell().Text($"{e.Amount:N2}");
                                        }
                                    });
                                }
                                else
                                {
                                    inner.Item().PaddingTop(5).Text("Nema evidentiranih stvarnih troškova (ili servis trenutno nije dostupan).")
                                        .FontSize(9).FontColor(Colors.Grey.Darken1);
                                }
                            });
                        });

                        page.Footer().AlignCenter().Text(text =>
                        {
                            text.CurrentPageNumber();
                            text.Span(" / ");
                            text.TotalPages();
                        });
                    });
                });

                var pdfBytes = document.GeneratePdf();
                var fileName = $"PutniPlan_{SanitizeFileName(plan.Name)}_{plan.Id}.pdf";

                _logger.LogInformation($"PDF izveštaj generisan za plan {travelPlanId}");

                return File(pdfBytes, "application/pdf", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error generating PDF for plan {travelPlanId}");
                return StatusCode(500, new { message = "Greška pri generisanju PDF izveštaja" });
            }
        }
    }
}