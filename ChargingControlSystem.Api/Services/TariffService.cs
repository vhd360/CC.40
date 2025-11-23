using ChargingControlSystem.Data;
using ChargingControlSystem.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace ChargingControlSystem.Api.Services;

public class TariffService : ITariffService
{
    private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
    private readonly ILogger<TariffService> _logger;

    public TariffService(
        IDbContextFactory<ApplicationDbContext> contextFactory,
        ILogger<TariffService> logger)
    {
        _contextFactory = contextFactory;
        _logger = logger;
    }

    public async Task<TariffCalculationResult> CalculateCostAsync(ChargingSession session)
    {
        if (!session.UserId.HasValue)
        {
            return new TariffCalculationResult
            {
                TotalCost = session.EnergyDelivered * 0.30m,
                Currency = "EUR",
                Breakdown = new Dictionary<string, decimal>
                {
                    { "Energy (default)", session.EnergyDelivered * 0.30m }
                }
            };
        }

        using var context = await _contextFactory.CreateDbContextAsync();

        // Load related entities if not already loaded
        if (session.ChargingPoint == null)
        {
            await context.Entry(session)
                .Reference(s => s.ChargingPoint)
                .Query()
                .Include(cp => cp.ChargingStation)
                .LoadAsync();
        }

        var stationId = session.ChargingPoint?.ChargingStation?.Id;
        if (!stationId.HasValue)
        {
            throw new InvalidOperationException("Cannot calculate cost without charging station information");
        }

        // Get applicable tariff
        var tariff = await GetApplicableTariffAsync(session.UserId.Value, stationId.Value);

        if (tariff == null)
        {
            return new TariffCalculationResult
            {
                TotalCost = session.EnergyDelivered * 0.30m,
                Currency = "EUR",
                Breakdown = new Dictionary<string, decimal>
                {
                    { "Energy (default)", session.EnergyDelivered * 0.30m }
                }
            };
        }

        // Load tariff components
        await context.Entry(tariff)
            .Collection(t => t.Components)
            .Query()
            .Where(c => c.IsActive)
            .OrderBy(c => c.DisplayOrder)
            .LoadAsync();

        var result = new TariffCalculationResult
        {
            Currency = tariff.Currency,
            AppliedTariff = tariff
        };

        var sessionEnd = session.EndedAt ?? DateTime.UtcNow;
        var chargingEnd = session.ChargingCompletedAt ?? sessionEnd;
        
        // Zeitberechnungen
        var chargingDurationMinutes = (int)(chargingEnd - session.StartedAt).TotalMinutes;
        var idleTimeMinutes = (int)(sessionEnd - chargingEnd).TotalMinutes;
        var totalParkingMinutes = (int)(sessionEnd - session.StartedAt).TotalMinutes;

        foreach (var component in tariff.Components.Where(c => c.IsActive))
        {
            decimal componentCost = 0m;

            switch (component.Type)
            {
                case TariffComponentType.Energy:
                    componentCost = CalculateEnergyCost(component, session.EnergyDelivered);
                    break;
                
                case TariffComponentType.ChargingTime:
                    componentCost = CalculateTimeCost(component, session.StartedAt, chargingEnd, chargingDurationMinutes);
                    break;
                
                case TariffComponentType.ParkingTime:
                    componentCost = CalculateTimeCost(component, session.StartedAt, sessionEnd, totalParkingMinutes);
                    break;
                
                case TariffComponentType.IdleTime:
                    // Nur Standzeit NACH Ladeende
                    componentCost = CalculateTimeCost(component, chargingEnd, sessionEnd, idleTimeMinutes);
                    break;
                
                case TariffComponentType.SessionFee:
                    componentCost = component.Price;
                    break;
                
                case TariffComponentType.TimeOfDay:
                    componentCost = CalculateTimeOfDayCost(component, session.StartedAt, sessionEnd, session.EnergyDelivered);
                    break;
            }

            // Apply minimum and maximum charges
            if (component.MinimumCharge.HasValue && componentCost < component.MinimumCharge.Value)
                componentCost = component.MinimumCharge.Value;

            if (component.MaximumCharge.HasValue && componentCost > component.MaximumCharge.Value)
                componentCost = component.MaximumCharge.Value;

            if (componentCost > 0)
            {
                result.Breakdown[$"{component.Type} ({component.Price:F4} {tariff.Currency})"] = componentCost;
                result.TotalCost += componentCost;
            }
        }

        return result;
    }

    public async Task<TariffCalculationResult> CalculateCostAsync(
        Guid userId,
        Guid chargingStationId,
        DateTime sessionStart,
        DateTime sessionEnd,
        decimal energyConsumed)
    {
        using var context = await _contextFactory.CreateDbContextAsync();

        // Get applicable tariff
        var tariff = await GetApplicableTariffAsync(userId, chargingStationId);

        if (tariff == null)
        {
            // No tariff found - use default pricing
            _logger.LogWarning("No tariff found for user {UserId}, using default pricing", userId);
            return new TariffCalculationResult
            {
                TotalCost = energyConsumed * 0.30m,
                Currency = "EUR",
                Breakdown = new Dictionary<string, decimal>
                {
                    { "Energy (default)", energyConsumed * 0.30m }
                }
            };
        }

        // Load tariff components
        await context.Entry(tariff)
            .Collection(t => t.Components)
            .Query()
            .Where(c => c.IsActive)
            .OrderBy(c => c.DisplayOrder)
            .LoadAsync();

        var result = new TariffCalculationResult
        {
            Currency = tariff.Currency,
            AppliedTariff = tariff
        };

        var sessionDuration = sessionEnd - sessionStart;
        var sessionMinutes = (int)sessionDuration.TotalMinutes;

        foreach (var component in tariff.Components.Where(c => c.IsActive))
        {
            decimal componentCost = component.Type switch
            {
                TariffComponentType.Energy => CalculateEnergyCost(component, energyConsumed),
                TariffComponentType.ChargingTime => CalculateTimeCost(component, sessionStart, sessionEnd, sessionMinutes),
                TariffComponentType.ParkingTime => CalculateTimeCost(component, sessionStart, sessionEnd, sessionMinutes),
                TariffComponentType.IdleTime => 0m, // Kann nicht ohne ChargingCompletedAt berechnet werden
                TariffComponentType.SessionFee => component.Price,
                TariffComponentType.TimeOfDay => CalculateTimeOfDayCost(component, sessionStart, sessionEnd, energyConsumed),
                _ => 0m
            };

            // Apply minimum and maximum charges
            if (component.MinimumCharge.HasValue && componentCost < component.MinimumCharge.Value)
                componentCost = component.MinimumCharge.Value;

            if (component.MaximumCharge.HasValue && componentCost > component.MaximumCharge.Value)
                componentCost = component.MaximumCharge.Value;

            if (componentCost > 0)
            {
                result.Breakdown[$"{component.Type} ({component.Price:F4} {tariff.Currency})"] = componentCost;
                result.TotalCost += componentCost;
            }
        }

        _logger.LogInformation(
            "Calculated cost for session: User={UserId}, Station={StationId}, Duration={Duration}min, Energy={Energy}kWh, Cost={Cost} {Currency}",
            userId, chargingStationId, sessionMinutes, energyConsumed, result.TotalCost, result.Currency);

        return result;
    }

    private decimal CalculateEnergyCost(TariffComponent component, decimal energyConsumed)
    {
        if (component.StepSize.HasValue && component.StepSize.Value > 0)
        {
            // Stepped billing (e.g., round up to next kWh)
            var steps = Math.Ceiling(energyConsumed / component.StepSize.Value);
            return steps * component.StepSize.Value * component.Price;
        }

        return energyConsumed * component.Price;
    }

    private decimal CalculateTimeCost(TariffComponent component, DateTime sessionStart, DateTime sessionEnd, int sessionMinutes)
    {
        var billableMinutes = sessionMinutes;

        // Apply grace period
        if (component.GracePeriodMinutes.HasValue)
        {
            billableMinutes = Math.Max(0, sessionMinutes - component.GracePeriodMinutes.Value);
        }

        // Check if component applies to specific days of week
        if (!string.IsNullOrEmpty(component.DaysOfWeek))
        {
            var applicableDays = component.DaysOfWeek.Split(',')
                .Select(d => int.Parse(d.Trim()))
                .ToList();

            var sessionDayOfWeek = (int)sessionStart.DayOfWeek;
            if (!applicableDays.Contains(sessionDayOfWeek))
            {
                return 0m; // Component doesn't apply on this day
            }
        }

        if (component.StepSize.HasValue && component.StepSize.Value > 0)
        {
            // Stepped billing (e.g., bill every 15 minutes)
            var steps = Math.Ceiling((decimal)billableMinutes / component.StepSize.Value);
            return steps * component.Price;
        }

        return billableMinutes * component.Price;
    }

    private decimal CalculateTimeOfDayCost(TariffComponent component, DateTime sessionStart, DateTime sessionEnd, decimal energyConsumed)
    {
        if (string.IsNullOrEmpty(component.TimeStart) || string.IsNullOrEmpty(component.TimeEnd))
        {
            return 0m;
        }

        // Parse time ranges
        var timeStart = TimeSpan.Parse(component.TimeStart);
        var timeEnd = TimeSpan.Parse(component.TimeEnd);

        var sessionStartTime = sessionStart.TimeOfDay;
        var sessionEndTime = sessionEnd.TimeOfDay;

        // Check if session overlaps with tariff time range
        bool isInRange = false;

        if (timeStart < timeEnd)
        {
            // Normal range (e.g., 08:00-20:00)
            isInRange = sessionStartTime >= timeStart && sessionEndTime <= timeEnd;
        }
        else
        {
            // Overnight range (e.g., 22:00-06:00)
            isInRange = sessionStartTime >= timeStart || sessionEndTime <= timeEnd;
        }

        if (isInRange)
        {
            return energyConsumed * component.Price;
        }

        return 0m;
    }

    public async Task<Tariff?> GetApplicableTariffAsync(Guid userId, Guid chargingStationId)
    {
        using var context = await _contextFactory.CreateDbContextAsync();

        // Get user's groups
        var userGroupIds = await context.UserGroupMemberships
            .Where(m => m.UserId == userId)
            .Select(m => m.UserGroupId)
            .ToListAsync();

        if (!userGroupIds.Any())
        {
            _logger.LogDebug("User {UserId} is not in any user groups", userId);
            
            // Get default tariff for user's tenant
            var user = await context.Users.FindAsync(userId);
            if (user != null)
            {
                return await GetDefaultTariffAsync(user.TenantId);
            }
            
            return null;
        }

        // Get tariffs assigned to user's groups (with highest priority)
        var tariff = await context.UserGroupTariffs
            .Include(ugt => ugt.Tariff)
                .ThenInclude(t => t.Components.Where(c => c.IsActive))
            .Where(ugt => userGroupIds.Contains(ugt.UserGroupId))
            .Where(ugt => ugt.Tariff.IsActive)
            .Where(ugt => (ugt.Tariff.ValidFrom == null || ugt.Tariff.ValidFrom <= DateTime.UtcNow))
            .Where(ugt => (ugt.Tariff.ValidUntil == null || ugt.Tariff.ValidUntil >= DateTime.UtcNow))
            .OrderByDescending(ugt => ugt.Priority)
            .Select(ugt => ugt.Tariff)
            .FirstOrDefaultAsync();

        if (tariff != null)
        {
            _logger.LogInformation("Found tariff {TariffId} ({TariffName}) for user {UserId}", 
                tariff.Id, tariff.Name, userId);
            return tariff;
        }

        // No group tariff found, try default tariff
        var userEntity = await context.Users.FindAsync(userId);
        if (userEntity != null)
        {
            return await GetDefaultTariffAsync(userEntity.TenantId);
        }

        return null;
    }

    public async Task<Tariff?> GetDefaultTariffAsync(Guid tenantId)
    {
        using var context = await _contextFactory.CreateDbContextAsync();

        var tariff = await context.Tariffs
            .Include(t => t.Components.Where(c => c.IsActive))
            .Where(t => t.TenantId == tenantId)
            .Where(t => t.IsDefault && t.IsActive)
            .Where(t => (t.ValidFrom == null || t.ValidFrom <= DateTime.UtcNow))
            .Where(t => (t.ValidUntil == null || t.ValidUntil >= DateTime.UtcNow))
            .FirstOrDefaultAsync();

        if (tariff != null)
        {
            _logger.LogInformation("Using default tariff {TariffId} ({TariffName}) for tenant {TenantId}", 
                tariff.Id, tariff.Name, tenantId);
        }

        return tariff;
    }
}

