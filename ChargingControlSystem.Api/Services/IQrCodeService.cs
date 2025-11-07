using ChargingControlSystem.Data.Entities;

namespace ChargingControlSystem.Api.Services;

public interface IQrCodeService
{
    Task<QrCode> GenerateQrCodeAsync(QrCodeType type, Guid? chargingParkId = null, Guid? userId = null);
    Task<QrCode?> ValidateQrCodeAsync(string code);
    Task<bool> UseQrCodeAsync(string code);
    Task<IEnumerable<QrCode>> GetQrCodesAsync();
}
