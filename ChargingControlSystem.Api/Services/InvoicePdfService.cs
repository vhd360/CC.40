using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using ChargingControlSystem.Data;
using ChargingControlSystem.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace ChargingControlSystem.Api.Services;

public interface IInvoicePdfService
{
    Task<byte[]> GenerateInvoicePdfAsync(Guid transactionId);
    Task<byte[]> GenerateMonthlySummaryPdfAsync(Guid userId, int year, int month);
}

public class InvoicePdfService : IInvoicePdfService
{
    private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
    private readonly ILogger<InvoicePdfService> _logger;

    public InvoicePdfService(
        IDbContextFactory<ApplicationDbContext> contextFactory,
        ILogger<InvoicePdfService> logger)
    {
        _contextFactory = contextFactory;
        _logger = logger;
        
        // QuestPDF License - Community License für nicht-kommerzielle Nutzung
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public async Task<byte[]> GenerateInvoicePdfAsync(Guid transactionId)
    {
        using var context = await _contextFactory.CreateDbContextAsync();

        var transaction = await context.BillingTransactions
            .Include(t => t.BillingAccount)
            .Include(t => t.ChargingSession)
                .ThenInclude(s => s!.ChargingPoint)
                    .ThenInclude(cp => cp.ChargingStation)
                        .ThenInclude(cs => cs.ChargingPark)
            .Include(t => t.ChargingSession)
                .ThenInclude(s => s!.Vehicle)
            .Include(t => t.ChargingSession)
                .ThenInclude(s => s!.User)
            .FirstOrDefaultAsync(t => t.Id == transactionId);

        if (transaction == null)
        {
            throw new InvalidOperationException($"Transaction {transactionId} not found");
        }

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(11));

                page.Header()
                    .Text($"Rechnung #{transaction.Id.ToString().Substring(0, 8).ToUpper()}")
                    .SemiBold().FontSize(20).FontColor(Colors.Blue.Medium);

                page.Content()
                    .PaddingVertical(1, Unit.Centimetre)
                    .Column(col =>
                    {
                        col.Spacing(20);

                        // Company & Customer Info
                        col.Item().Row(row =>
                        {
                            row.RelativeItem().Column(c =>
                            {
                                c.Item().Text("CUBOS.Charge").SemiBold().FontSize(14);
                                c.Item().Text("Lade-Management-System");
                                c.Item().Text($"Rechnungsdatum: {transaction.CreatedAt:dd.MM.yyyy}");
                            });

                            row.RelativeItem().Column(c =>
                            {
                                c.Item().Text("Kunde:").SemiBold();
                                c.Item().Text(transaction.BillingAccount.AccountName);
                                if (transaction.ChargingSession?.User != null)
                                {
                                    var user = transaction.ChargingSession.User;
                                    c.Item().Text($"{user.FirstName} {user.LastName}");
                                    c.Item().Text(user.Email);
                                }
                            });
                        });

                        // Transaction Details
                        col.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                        
                        col.Item().Text("Ladevorgang Details").SemiBold().FontSize(14);
                        
                        if (transaction.ChargingSession != null)
                        {
                            var session = transaction.ChargingSession;
                            var station = session.ChargingPoint?.ChargingStation;
                            var park = station?.ChargingPark;

                            col.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.ConstantColumn(150);
                                    columns.RelativeColumn();
                                });

                                table.Cell().Text("Ladestation:");
                                table.Cell().Text(station?.Name ?? "N/A");

                                table.Cell().Text("Standort:");
                                table.Cell().Text(park != null ? $"{park.Name}, {park.City}" : "N/A");

                                if (session.Vehicle != null)
                                {
                                    table.Cell().Text("Fahrzeug:");
                                    table.Cell().Text($"{session.Vehicle.Make} {session.Vehicle.Model} ({session.Vehicle.LicensePlate})");
                                }

                                table.Cell().Text("Startzeit:");
                                table.Cell().Text(session.StartedAt.ToString("dd.MM.yyyy HH:mm:ss"));

                                table.Cell().Text("Endzeit:");
                                table.Cell().Text(session.EndedAt?.ToString("dd.MM.yyyy HH:mm:ss") ?? "N/A");

                                var duration = session.EndedAt.HasValue 
                                    ? (session.EndedAt.Value - session.StartedAt).TotalMinutes 
                                    : 0;
                                table.Cell().Text("Dauer:");
                                table.Cell().Text($"{duration:F0} Minuten");

                                table.Cell().Text("Geladene Energie:");
                                table.Cell().Text($"{session.EnergyDelivered:F2} kWh").SemiBold();
                            });
                        }

                        // Cost Breakdown
                        col.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                        
                        col.Item().Text("Kostenaufstellung").SemiBold().FontSize(14);
                        
                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(3);
                                columns.RelativeColumn(1);
                            });

                            // Header
                            table.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Position").SemiBold();
                            table.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Betrag").SemiBold();

                            // Items
                            table.Cell().Padding(5).Text(transaction.Description);
                            table.Cell().Padding(5).Text($"{transaction.Amount:F2} {transaction.Currency}").AlignRight();
                        });

                        // Total
                        col.Item().AlignRight().Column(c =>
                        {
                            c.Item().LineHorizontal(2).LineColor(Colors.Blue.Medium);
                            c.Item().PaddingTop(5).Row(row =>
                            {
                                row.RelativeItem().Text("Gesamtbetrag:").SemiBold().FontSize(14);
                                row.ConstantItem(100).Text($"{transaction.Amount:F2} {transaction.Currency}").SemiBold().FontSize(14).AlignRight();
                            });
                        });

                        // Payment Status
                        col.Item().PaddingTop(10).Row(row =>
                        {
                            row.AutoItem().Text("Status: ").SemiBold();
                            var statusText = transaction.Status switch
                            {
                                BillingTransactionStatus.Pending => "Offen",
                                BillingTransactionStatus.Completed => "Bezahlt",
                                BillingTransactionStatus.Refunded => "Erstattet",
                                _ => transaction.Status.ToString()
                            };
                            row.AutoItem().Text(statusText).FontColor(
                                transaction.Status == BillingTransactionStatus.Completed 
                                    ? Colors.Green.Medium 
                                    : Colors.Orange.Medium);
                        });

                        if (transaction.ProcessedAt.HasValue)
                        {
                            col.Item().Text($"Bezahlt am: {transaction.ProcessedAt:dd.MM.yyyy HH:mm}");
                        }
                    });

                page.Footer()
                    .AlignCenter()
                    .DefaultTextStyle(x => x.FontSize(9).FontColor(Colors.Grey.Medium))
                    .Text(text =>
                    {
                        text.Span("CUBOS.Charge - Vielen Dank für Ihre Nutzung! | ");
                        text.Span($"Seite ");
                        text.CurrentPageNumber();
                        text.Span(" von ");
                        text.TotalPages();
                    });
            });
        });

        return document.GeneratePdf();
    }

    public async Task<byte[]> GenerateMonthlySummaryPdfAsync(Guid userId, int year, int month)
    {
        using var context = await _contextFactory.CreateDbContextAsync();

        var startDate = new DateTime(year, month, 1);
        var endDate = startDate.AddMonths(1);

        var transactions = await context.BillingTransactions
            .Include(t => t.BillingAccount)
            .Include(t => t.ChargingSession)
                .ThenInclude(s => s!.ChargingPoint)
                    .ThenInclude(cp => cp.ChargingStation)
            .Where(t => t.ChargingSession!.UserId == userId &&
                       t.CreatedAt >= startDate &&
                       t.CreatedAt < endDate)
            .OrderBy(t => t.CreatedAt)
            .ToListAsync();

        var user = await context.Users.FindAsync(userId);
        if (user == null)
        {
            throw new InvalidOperationException($"User {userId} not found");
        }

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(11));

                page.Header()
                    .Column(col =>
                    {
                        col.Item().Text($"Monatsabrechnung {month:D2}/{year}").SemiBold().FontSize(20).FontColor(Colors.Blue.Medium);
                        col.Item().Text($"{user.FirstName} {user.LastName}").FontSize(14);
                    });

                page.Content()
                    .PaddingVertical(1, Unit.Centimetre)
                    .Column(col =>
                    {
                        col.Spacing(20);

                        // Summary
                        var totalAmount = transactions.Sum(t => t.Amount);
                        var totalEnergy = transactions.Sum(t => t.ChargingSession?.EnergyDelivered ?? 0);
                        var sessionCount = transactions.Count;

                        col.Item().Background(Colors.Blue.Lighten4).Padding(10).Column(c =>
                        {
                            c.Item().Text("Zusammenfassung").SemiBold().FontSize(14);
                            c.Item().Row(row =>
                            {
                                row.RelativeItem().Text($"Anzahl Ladevorgänge: {sessionCount}");
                                row.RelativeItem().Text($"Gesamt Energie: {totalEnergy:F2} kWh");
                                row.RelativeItem().Text($"Gesamtbetrag: {totalAmount:F2} EUR").SemiBold();
                            });
                        });

                        // Transactions Table
                        col.Item().Text("Ladevorgänge").SemiBold().FontSize(14);
                        
                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(80);
                                columns.RelativeColumn(2);
                                columns.ConstantColumn(60);
                                columns.ConstantColumn(70);
                                columns.ConstantColumn(60);
                            });

                            // Header
                            table.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Datum").SemiBold();
                            table.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Station").SemiBold();
                            table.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Energie").SemiBold();
                            table.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Betrag").SemiBold();
                            table.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Status").SemiBold();

                            // Rows
                            foreach (var t in transactions)
                            {
                                table.Cell().Padding(5).Text(t.CreatedAt.ToString("dd.MM.yy"));
                                table.Cell().Padding(5).Text(t.ChargingSession?.ChargingPoint?.ChargingStation?.Name ?? "N/A");
                                table.Cell().Padding(5).Text($"{t.ChargingSession?.EnergyDelivered:F1} kWh");
                                table.Cell().Padding(5).Text($"{t.Amount:F2} €");
                                table.Cell().Padding(5).Text(t.Status == BillingTransactionStatus.Completed ? "Bezahlt" : "Offen");
                            }
                        });

                        // Total
                        col.Item().AlignRight().Column(c =>
                        {
                            c.Item().LineHorizontal(2).LineColor(Colors.Blue.Medium);
                            c.Item().PaddingTop(5).Row(row =>
                            {
                                row.RelativeItem().Text("Gesamtbetrag:").SemiBold().FontSize(14);
                                row.ConstantItem(100).Text($"{totalAmount:F2} EUR").SemiBold().FontSize(14).AlignRight();
                            });
                        });
                    });

                page.Footer()
                    .AlignCenter()
                    .DefaultTextStyle(x => x.FontSize(9).FontColor(Colors.Grey.Medium))
                    .Text(text =>
                    {
                        text.Span("CUBOS.Charge - Monatsabrechnung | ");
                        text.Span($"Seite ");
                        text.CurrentPageNumber();
                        text.Span(" von ");
                        text.TotalPages();
                    });
            });
        });

        return document.GeneratePdf();
    }
}

