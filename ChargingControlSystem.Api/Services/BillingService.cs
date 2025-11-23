using ChargingControlSystem.Data;
using ChargingControlSystem.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace ChargingControlSystem.Api.Services;

public class BillingService : IBillingService
{
    private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
    private readonly ILogger<BillingService> _logger;

    public BillingService(
        IDbContextFactory<ApplicationDbContext> contextFactory,
        ILogger<BillingService> logger)
    {
        _contextFactory = contextFactory;
        _logger = logger;
    }

    public async Task<BillingTransaction> CreateTransactionForSessionAsync(ChargingSession session)
    {
        using var context = await _contextFactory.CreateDbContextAsync();

        try
        {
            // Reload session with all navigation properties from THIS context
            var fullSession = await context.ChargingSessions
                .Include(s => s.ChargingPoint)
                    .ThenInclude(cp => cp.ChargingStation)
                .Include(s => s.Vehicle)
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.Id == session.Id);

            if (fullSession == null)
            {
                _logger.LogError("Session {SessionId} not found when creating billing transaction", session.Id);
                throw new InvalidOperationException($"Session {session.Id} not found");
            }

            // Hole oder erstelle BillingAccount in THIS context
            BillingAccount billingAccount;
            
            if (fullSession.UserId.HasValue)
            {
                billingAccount = await GetOrCreateBillingAccountInContextAsync(context, fullSession.UserId.Value, fullSession.TenantId);
            }
            else
            {
                // Adhoc-Session: Erstelle temporären Account oder nutze Pool-Account
                billingAccount = await GetOrCreatePoolAccountInContextAsync(context, fullSession.TenantId);
            }

            // Erstelle Transaktion mit vollständigen Daten
            var stationName = fullSession.ChargingPoint?.ChargingStation?.Name ?? "Station";
            var vehicleInfo = fullSession.Vehicle != null 
                ? $" ({fullSession.Vehicle.Make} {fullSession.Vehicle.Model} - {fullSession.Vehicle.LicensePlate})" 
                : "";

            var transaction = new BillingTransaction
            {
                Id = Guid.NewGuid(),
                BillingAccountId = billingAccount.Id,
                ChargingSessionId = fullSession.Id,
                TransactionType = "charging",
                Amount = fullSession.Cost,
                Currency = "EUR",
                Status = BillingTransactionStatus.Pending, // Pending bis zur Bezahlung
                Description = $"Ladevorgang an {stationName}{vehicleInfo} - {fullSession.EnergyDelivered:F2} kWh",
                CreatedAt = DateTime.UtcNow,
                ProcessedAt = null
            };

            context.BillingTransactions.Add(transaction);
            await context.SaveChangesAsync();

            _logger.LogInformation(
                "Created billing transaction {TransactionId} for session {SessionId}: {Amount} {Currency}",
                transaction.Id, fullSession.Id, transaction.Amount, transaction.Currency);

            return transaction;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating billing transaction for session {SessionId}", session.Id);
            throw;
        }
    }

    private async Task<BillingAccount> GetOrCreateBillingAccountInContextAsync(
        ApplicationDbContext context, 
        Guid userId, 
        Guid tenantId)
    {
        // Suche existierenden Account für diesen User
        var existingAccount = await context.BillingAccounts
            .FirstOrDefaultAsync(ba => 
                ba.TenantId == tenantId && 
                ba.AccountName.Contains(userId.ToString())); // Temporäre Lösung

        if (existingAccount != null)
        {
            return existingAccount;
        }

        // Bessere Suche: Finde Account über Transaktionen
        var accountFromTransaction = await context.BillingTransactions
            .Where(t => t.ChargingSession!.UserId == userId && t.BillingAccount.TenantId == tenantId)
            .Select(t => t.BillingAccount)
            .FirstOrDefaultAsync();

        if (accountFromTransaction != null)
        {
            return accountFromTransaction;
        }

        // Hole User-Daten
        var user = await context.Users.FindAsync(userId);
        if (user == null)
        {
            throw new InvalidOperationException($"User {userId} not found");
        }

        // Erstelle neuen Account
        var account = new BillingAccount
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            AccountName = $"{user.FirstName} {user.LastName} ({userId})", // Include userId for lookup
            Type = BillingAccountType.Individual,
            Status = BillingAccountStatus.Active,
            CreatedAt = DateTime.UtcNow
        };

        context.BillingAccounts.Add(account);
        await context.SaveChangesAsync();

        _logger.LogInformation("Created billing account {AccountId} for user {UserId}", account.Id, userId);

        return account;
    }

    public async Task<BillingAccount> GetOrCreateBillingAccountAsync(Guid userId, Guid tenantId)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        return await GetOrCreateBillingAccountInContextAsync(context, userId, tenantId);
    }

    public async Task<IEnumerable<BillingTransaction>> GetTransactionsForUserAsync(Guid userId)
    {
        using var context = await _contextFactory.CreateDbContextAsync();

        return await context.BillingTransactions
            .Include(t => t.ChargingSession)
                .ThenInclude(s => s!.ChargingPoint)
                    .ThenInclude(cp => cp.ChargingStation)
            .Where(t => t.ChargingSession!.UserId == userId)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<BillingTransaction>> GetTransactionsForTenantAsync(
        Guid tenantId,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        BillingTransactionStatus? status = null)
    {
        using var context = await _contextFactory.CreateDbContextAsync();

        var query = context.BillingTransactions
            .Include(t => t.BillingAccount)
            .Include(t => t.ChargingSession)
                .ThenInclude(s => s!.User)
            .Include(t => t.ChargingSession)
                .ThenInclude(s => s!.ChargingPoint)
                    .ThenInclude(cp => cp.ChargingStation)
            .Where(t => t.BillingAccount.TenantId == tenantId);

        if (fromDate.HasValue)
        {
            query = query.Where(t => t.CreatedAt >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(t => t.CreatedAt <= toDate.Value);
        }

        if (status.HasValue)
        {
            query = query.Where(t => t.Status == status.Value);
        }

        return await query.OrderByDescending(t => t.CreatedAt).ToListAsync();
    }

    public async Task<decimal> CalculateTotalCostAsync(Guid userId, DateTime? fromDate = null, DateTime? toDate = null)
    {
        using var context = await _contextFactory.CreateDbContextAsync();

        var query = context.BillingTransactions
            .Where(t => t.ChargingSession!.UserId == userId)
            .Where(t => t.Status == BillingTransactionStatus.Completed);

        if (fromDate.HasValue)
        {
            query = query.Where(t => t.CreatedAt >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(t => t.CreatedAt <= toDate.Value);
        }

        return await query.SumAsync(t => t.Amount);
    }

    public async Task<bool> MarkTransactionAsPaidAsync(Guid transactionId)
    {
        using var context = await _contextFactory.CreateDbContextAsync();

        var transaction = await context.BillingTransactions.FindAsync(transactionId);
        if (transaction == null)
        {
            return false;
        }

        transaction.Status = BillingTransactionStatus.Completed;
        transaction.ProcessedAt = DateTime.UtcNow;

        await context.SaveChangesAsync();

        _logger.LogInformation("Marked transaction {TransactionId} as paid", transactionId);

        return true;
    }

    public async Task<bool> RefundTransactionAsync(Guid transactionId, string reason)
    {
        using var context = await _contextFactory.CreateDbContextAsync();

        var transaction = await context.BillingTransactions.FindAsync(transactionId);
        if (transaction == null)
        {
            return false;
        }

        transaction.Status = BillingTransactionStatus.Refunded;
        transaction.Description += $" | STORNIERT: {reason}";
        transaction.ProcessedAt = DateTime.UtcNow;

        await context.SaveChangesAsync();

        _logger.LogInformation("Refunded transaction {TransactionId}: {Reason}", transactionId, reason);

        return true;
    }

    private async Task<BillingAccount> GetOrCreatePoolAccountInContextAsync(
        ApplicationDbContext context,
        Guid tenantId)
    {
        // Suche Pool-Account für Adhoc-Ladungen
        var poolAccount = await context.BillingAccounts
            .FirstOrDefaultAsync(ba => ba.TenantId == tenantId && ba.Type == BillingAccountType.PoolAccount);

        if (poolAccount != null)
        {
            return poolAccount;
        }

        // Erstelle Pool-Account
        var account = new BillingAccount
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            AccountName = "Adhoc / Gäste",
            Type = BillingAccountType.PoolAccount,
            Status = BillingAccountStatus.Active,
            CreatedAt = DateTime.UtcNow
        };

        context.BillingAccounts.Add(account);
        await context.SaveChangesAsync();

        _logger.LogInformation("Created pool billing account {AccountId} for tenant {TenantId}", account.Id, tenantId);

        return account;
    }

    private async Task<BillingAccount> GetOrCreatePoolAccountAsync(Guid tenantId)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        return await GetOrCreatePoolAccountInContextAsync(context, tenantId);
    }
}
