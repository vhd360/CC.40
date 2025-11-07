using ChargingControlSystem.Api.Services;
using ChargingControlSystem.Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChargingControlSystem.Api.Controllers;

[ApiController]
[Route("api/qrcodes")]
[Authorize]
public class QrCodeController : ControllerBase
{
    private readonly IQrCodeService _qrCodeService;

    public QrCodeController(IQrCodeService qrCodeService)
    {
        _qrCodeService = qrCodeService;
    }

    [HttpPost("generate")]
    public async Task<IActionResult> GenerateQrCode([FromQuery] QrCodeType type, [FromQuery] Guid? chargingParkId, [FromQuery] Guid? userId)
    {
        var qrCode = await _qrCodeService.GenerateQrCodeAsync(type, chargingParkId, userId);
        return Ok(qrCode);
    }

    [HttpGet("validate/{code}")]
    [AllowAnonymous]
    public async Task<IActionResult> ValidateQrCode(string code)
    {
        var qrCode = await _qrCodeService.ValidateQrCodeAsync(code);
        if (qrCode == null)
        {
            return NotFound();
        }
        return Ok(qrCode);
    }

    [HttpPost("use/{code}")]
    [AllowAnonymous]
    public async Task<IActionResult> UseQrCode(string code)
    {
        var success = await _qrCodeService.UseQrCodeAsync(code);
        if (!success)
        {
            return BadRequest("Invalid or expired QR code");
        }
        return Ok();
    }

    [HttpGet]
    public async Task<IActionResult> GetQrCodes()
    {
        var qrCodes = await _qrCodeService.GetQrCodesAsync();
        return Ok(qrCodes);
    }
}
