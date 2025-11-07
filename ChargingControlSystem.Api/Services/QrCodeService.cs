using ChargingControlSystem.Data;
using ChargingControlSystem.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace ChargingControlSystem.Api.Services;

public class QrCodeService : IQrCodeService
{
    private readonly ApplicationDbContext _context;
    private readonly ITenantService _tenantService;

    public QrCodeService(ApplicationDbContext context, ITenantService tenantService)
    {
        _context = context;
        _tenantService = tenantService;
    }

    public async Task<QrCode> GenerateQrCodeAsync(QrCodeType type, Guid? chargingParkId = null, Guid? userId = null)
    {
        var tenant = await _tenantService.GetCurrentTenantAsync();
        if (tenant == null)
        {
            throw new InvalidOperationException("Tenant not found");
        }

        var qrCode = new QrCode
        {
            Id = Guid.NewGuid(),
            TenantId = tenant.Id,
            Code = GenerateUniqueCode(),
            Type = type,
            ChargingParkId = chargingParkId,
            UserId = userId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(365) // Default 1 year expiry
        };

        _context.QrCodes.Add(qrCode);
        await _context.SaveChangesAsync();

        return qrCode;
    }

    public async Task<QrCode?> ValidateQrCodeAsync(string code)
    {
        return await _context.QrCodes
            .Include(qr => qr.ChargingPark)
            .Include(qr => qr.User)
            .FirstOrDefaultAsync(qr =>
                qr.Code == code &&
                qr.IsActive &&
                (qr.ExpiresAt == null || qr.ExpiresAt > DateTime.UtcNow) &&
                (qr.MaxUses == null || qr.CurrentUses < qr.MaxUses));
    }

    public async Task<bool> UseQrCodeAsync(string code)
    {
        var qrCode = await ValidateQrCodeAsync(code);
        if (qrCode == null)
        {
            return false;
        }

        qrCode.CurrentUses++;
        qrCode.LastUsedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<QrCode>> GetQrCodesAsync()
    {
        var tenant = await _tenantService.GetCurrentTenantAsync();
        if (tenant == null) return new List<QrCode>();

        return await _context.QrCodes
            .Where(qr => qr.TenantId == tenant.Id)
            .Include(qr => qr.ChargingPark)
            .Include(qr => qr.User)
            .ToListAsync();
    }

    private string GenerateUniqueCode()
    {
        string code;
        do
        {
            code = Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper();
        }
        while (_context.QrCodes.Any(qr => qr.Code == code));

        return code;
    }
}
