using ChargingControlSystem.Data.Entities;

namespace ChargingControlSystem.Api.Services;

public interface IBillingService
{
    /// <summary>
    /// Erstellt automatisch eine BillingTransaction für eine abgeschlossene ChargingSession
    /// </summary>
    Task<BillingTransaction> CreateTransactionForSessionAsync(ChargingSession session);

    /// <summary>
    /// Holt oder erstellt ein BillingAccount für einen User
    /// </summary>
    Task<BillingAccount> GetOrCreateBillingAccountAsync(Guid userId, Guid tenantId);

    /// <summary>
    /// Holt alle Transaktionen für einen User
    /// </summary>
    Task<IEnumerable<BillingTransaction>> GetTransactionsForUserAsync(Guid userId);

    /// <summary>
    /// Holt alle Transaktionen für einen Tenant (mit Filterung)
    /// </summary>
    Task<IEnumerable<BillingTransaction>> GetTransactionsForTenantAsync(
        Guid tenantId, 
        DateTime? fromDate = null, 
        DateTime? toDate = null,
        BillingTransactionStatus? status = null);

    /// <summary>
    /// Berechnet die Gesamtkosten für einen User in einem Zeitraum
    /// </summary>
    Task<decimal> CalculateTotalCostAsync(Guid userId, DateTime? fromDate = null, DateTime? toDate = null);

    /// <summary>
    /// Markiert eine Transaktion als bezahlt
    /// </summary>
    Task<bool> MarkTransactionAsPaidAsync(Guid transactionId);

    /// <summary>
    /// Storniert eine Transaktion
    /// </summary>
    Task<bool> RefundTransactionAsync(Guid transactionId, string reason);
}
