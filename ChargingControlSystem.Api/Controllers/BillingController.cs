using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ChargingControlSystem.Data;
using ChargingControlSystem.Api.Services;
using ChargingControlSystem.Data.Entities;
using Swashbuckle.AspNetCore.Annotations;

namespace ChargingControlSystem.Api.Controllers;

[ApiController]
[Route("api/billing")]
public class BillingController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IBillingService _billingService;
    private readonly ITenantService _tenantService;
    private readonly IInvoicePdfService _invoicePdfService;
    private readonly ILogger<BillingController> _logger;

    public BillingController(
        ApplicationDbContext context,
        IBillingService billingService,
        ITenantService tenantService,
        IInvoicePdfService invoicePdfService,
        ILogger<BillingController> logger)
    {
        _context = context;
        _billingService = billingService;
        _tenantService = tenantService;
        _invoicePdfService = invoicePdfService;
        _logger = logger;
    }

    [HttpGet("transactions")]
    [SwaggerOperation(Summary = "Alle Transaktionen abrufen", Description = "Ruft alle Transaktionen für den aktuellen Tenant ab mit optionaler Filterung")]
    [SwaggerResponse(200, "Transaktionen erfolgreich abgerufen")]
    public async Task<IActionResult> GetTransactions(
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        [FromQuery] BillingTransactionStatus? status = null)
    {
        var tenantId = _tenantService.GetCurrentTenantId();

        var transactions = await _billingService.GetTransactionsForTenantAsync(
            tenantId, fromDate, toDate, status);

        var result = transactions.Select(t => new
        {
            t.Id,
            t.Amount,
            t.Currency,
            t.Description,
            Status = t.Status.ToString(),
            t.TransactionType,
            t.CreatedAt,
            t.ProcessedAt,
            Account = new
            {
                t.BillingAccount.Id,
                t.BillingAccount.AccountName
            },
            Session = t.ChargingSession != null ? new
            {
                t.ChargingSession.Id,
                t.ChargingSession.SessionId,
                t.ChargingSession.EnergyDelivered,
                User = t.ChargingSession.User != null 
                    ? $"{t.ChargingSession.User.FirstName} {t.ChargingSession.User.LastName}"
                    : "Adhoc",
                Station = t.ChargingSession.ChargingConnector?.ChargingPoint?.ChargingStation?.Name ?? "Unknown"
            } : null
        });

        return Ok(result);
    }

    [HttpGet("transactions/{id}")]
    [SwaggerOperation(Summary = "Transaction-Details abrufen", Description = "Ruft eine einzelne Transaktion mit allen Details ab")]
    [SwaggerResponse(200, "Transaktion gefunden")]
    [SwaggerResponse(404, "Transaktion nicht gefunden")]
    public async Task<IActionResult> GetTransaction(Guid id)
    {
        var transaction = await _context.BillingTransactions
            .Include(t => t.BillingAccount)
            .Include(t => t.ChargingSession)
                .ThenInclude(s => s!.User)
            .Include(t => t.ChargingSession)
                .ThenInclude(s => s!.ChargingConnector)
                    .ThenInclude(c => c.ChargingPoint)
                        .ThenInclude(cp => cp.ChargingStation)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (transaction == null)
            return NotFound();

        return Ok(transaction);
    }

    [HttpGet("accounts")]
    [SwaggerOperation(Summary = "Alle Billing-Accounts abrufen", Description = "Ruft alle Billing-Accounts für den aktuellen Tenant ab")]
    [SwaggerResponse(200, "Accounts erfolgreich abgerufen")]
    public async Task<IActionResult> GetAccounts()
    {
        var tenantId = _tenantService.GetCurrentTenantId();

        var accounts = await _context.BillingAccounts
            .Where(a => a.TenantId == tenantId)
            .Select(a => new
            {
                a.Id,
                a.AccountName,
                Type = a.Type.ToString(),
                Status = a.Status.ToString(),
                a.CreatedAt,
                TransactionCount = a.Transactions.Count,
                TotalAmount = a.Transactions
                    .Where(t => t.Status == BillingTransactionStatus.Completed)
                    .Sum(t => t.Amount)
            })
            .ToListAsync();

        return Ok(accounts);
    }

    [HttpGet("summary")]
    [SwaggerOperation(Summary = "Abrechnungs-Zusammenfassung", Description = "Ruft eine Zusammenfassung aller Abrechnungen ab")]
    [SwaggerResponse(200, "Zusammenfassung erfolgreich abgerufen")]
    public async Task<IActionResult> GetSummary()
    {
        var tenantId = _tenantService.GetCurrentTenantId();

        var now = DateTime.UtcNow;
        var startOfMonth = new DateTime(now.Year, now.Month, 1);
        var startOfYear = new DateTime(now.Year, 1, 1);

        var allTransactions = await _billingService.GetTransactionsForTenantAsync(tenantId);
        var completedTransactions = allTransactions.Where(t => t.Status == BillingTransactionStatus.Completed).ToList();

        var summary = new
        {
            TotalRevenue = completedTransactions.Sum(t => t.Amount),
            TotalTransactions = allTransactions.Count(),
            CompletedTransactions = completedTransactions.Count,
            PendingTransactions = allTransactions.Count(t => t.Status == BillingTransactionStatus.Pending),
            
            // This month
            MonthlyRevenue = completedTransactions
                .Where(t => t.CreatedAt >= startOfMonth)
                .Sum(t => t.Amount),
            MonthlyTransactions = allTransactions
                .Count(t => t.CreatedAt >= startOfMonth),
            
            // This year
            YearlyRevenue = completedTransactions
                .Where(t => t.CreatedAt >= startOfYear)
                .Sum(t => t.Amount),
            YearlyTransactions = allTransactions
                .Count(t => t.CreatedAt >= startOfYear),
            
            // Average
            AverageTransactionValue = completedTransactions.Any() 
                ? completedTransactions.Average(t => t.Amount)
                : 0,
            
            Currency = "EUR"
        };

        return Ok(summary);
    }

    [HttpPost("transactions/{id}/mark-paid")]
    [SwaggerOperation(Summary = "Transaktion als bezahlt markieren", Description = "Markiert eine ausstehende Transaktion als bezahlt")]
    [SwaggerResponse(200, "Transaktion als bezahlt markiert")]
    [SwaggerResponse(404, "Transaktion nicht gefunden")]
    public async Task<IActionResult> MarkAsPaid(Guid id)
    {
        var success = await _billingService.MarkTransactionAsPaidAsync(id);
        
        if (!success)
            return NotFound();

        return Ok(new { message = "Transaktion wurde als bezahlt markiert" });
    }

    [HttpPost("transactions/{id}/refund")]
    [SwaggerOperation(Summary = "Transaktion stornieren", Description = "Storniert eine Transaktion und erstattet den Betrag")]
    [SwaggerResponse(200, "Transaktion wurde storniert")]
    [SwaggerResponse(404, "Transaktion nicht gefunden")]
    public async Task<IActionResult> Refund(Guid id, [FromBody] RefundRequest request)
    {
        var success = await _billingService.RefundTransactionAsync(id, request.Reason);
        
        if (!success)
            return NotFound();

        return Ok(new { message = "Transaktion wurde storniert" });
    }

    [HttpGet("transactions/{id}/pdf")]
    [SwaggerOperation(Summary = "Rechnung als PDF herunterladen", Description = "Generiert und lädt eine Rechnung als PDF-Datei herunter")]
    [SwaggerResponse(200, "PDF erfolgreich generiert", typeof(FileContentResult))]
    [SwaggerResponse(404, "Transaktion nicht gefunden")]
    public async Task<IActionResult> DownloadInvoicePdf(Guid id)
    {
        try
        {
            var pdfBytes = await _invoicePdfService.GenerateInvoicePdfAsync(id);
            return File(pdfBytes, "application/pdf", $"Rechnung_{id.ToString().Substring(0, 8)}.pdf");
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating invoice PDF for transaction {TransactionId}", id);
            return StatusCode(500, new { error = "Fehler beim Erstellen der PDF" });
        }
    }

    [HttpGet("monthly-summary/pdf")]
    [SwaggerOperation(Summary = "Monatsabrechnung als PDF herunterladen", Description = "Generiert eine Monatsabrechnung als PDF")]
    [SwaggerResponse(200, "PDF erfolgreich generiert", typeof(FileContentResult))]
    public async Task<IActionResult> DownloadMonthlySummaryPdf(
        [FromQuery] int year,
        [FromQuery] int month)
    {
        try
        {
            // Get current user ID from JWT token
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
                return Unauthorized();

            var userId = Guid.Parse(userIdClaim.Value);
            var pdfBytes = await _invoicePdfService.GenerateMonthlySummaryPdfAsync(userId, year, month);
            
            return File(pdfBytes, "application/pdf", $"Monatsabrechnung_{year}_{month:D2}.pdf");
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating monthly summary PDF");
            return StatusCode(500, new { error = "Fehler beim Erstellen der PDF" });
        }
    }
}

public record RefundRequest(string Reason);

