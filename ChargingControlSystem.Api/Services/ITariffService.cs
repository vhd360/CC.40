using ChargingControlSystem.Data.Entities;

namespace ChargingControlSystem.Api.Services;

public interface ITariffService
{
    /// <summary>
    /// Calculate cost for a charging session based on user's tariff
    /// </summary>
    Task<TariffCalculationResult> CalculateCostAsync(
        Guid userId,
        Guid chargingStationId,
        DateTime sessionStart,
        DateTime sessionEnd,
        decimal energyConsumed);

    /// <summary>
    /// Calculate cost for a charging session (mit ChargingCompletedAt für korrekte Standzeit-Berechnung)
    /// </summary>
    Task<TariffCalculationResult> CalculateCostAsync(ChargingSession session);

    /// <summary>
    /// Get applicable tariff for a user at a specific charging station
    /// </summary>
    Task<Tariff?> GetApplicableTariffAsync(Guid userId, Guid chargingStationId);

    /// <summary>
    /// Get default tariff for a tenant
    /// </summary>
    Task<Tariff?> GetDefaultTariffAsync(Guid tenantId);
}

public class TariffCalculationResult
{
    public decimal TotalCost { get; set; }
    public string Currency { get; set; } = "EUR";
    public Dictionary<string, decimal> Breakdown { get; set; } = new();
    public Tariff? AppliedTariff { get; set; }
}

