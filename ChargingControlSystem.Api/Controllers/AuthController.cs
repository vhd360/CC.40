using ChargingControlSystem.Api.Models;
using ChargingControlSystem.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace ChargingControlSystem.Api.Controllers;

/// <summary>
/// Authentifizierung und Benutzerverwaltung
/// </summary>
[ApiController]
[Route("api/auth")]
[Produces("application/json")]
[SwaggerTag("Authentifizierungs-Endpoints für Login, Registrierung und Token-Verwaltung")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// Benutzer-Login
    /// </summary>
    /// <param name="request">Login-Daten (Email und Passwort)</param>
    /// <returns>JWT Access Token und Refresh Token</returns>
    /// <remarks>
    /// Beispiel Request:
    /// 
    ///     POST /api/auth/login
    ///     {
    ///       "email": "admin@chargingcontrol.com",
    ///       "password": "admin123"
    ///     }
    /// 
    /// Erfolgreiche Response enthält:
    /// - **accessToken**: JWT Token für API-Authentifizierung (gültig 24h)
    /// - **refreshToken**: Token zum Erneuern des Access Tokens
    /// - **user**: Benutzerinformationen (Id, Email, Name, Rolle)
    /// 
    /// **Standard-Benutzer (Seed-Daten):**
    /// - Email: `admin@chargingcontrol.com`
    /// - Passwort: `admin123`
    /// </remarks>
    /// <response code="200">Login erfolgreich - JWT Token wird zurückgegeben</response>
    /// <response code="400">Ungültige Anmeldedaten oder Benutzer nicht aktiv</response>
    /// <response code="401">Email oder Passwort falsch</response>
    [HttpPost("login")]
    [SwaggerOperation(
        Summary = "Benutzer anmelden",
        Description = "Authentifiziert einen Benutzer mit Email und Passwort und gibt JWT Tokens zurück.",
        OperationId = "Login",
        Tags = new[] { "Authentifizierung" }
    )]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var result = await _authService.LoginAsync(request);
        if (!result.Success)
        {
            return BadRequest(result);
        }
        return Ok(result);
    }

    /// <summary>
    /// Neuen Benutzer registrieren
    /// </summary>
    /// <param name="request">Registrierungsdaten</param>
    /// <returns>JWT Token für den neuen Benutzer</returns>
    /// <remarks>
    /// Beispiel Request:
    /// 
    ///     POST /api/auth/register
    ///     {
    ///       "email": "user@example.com",
    ///       "password": "SecurePassword123!",
    ///       "firstName": "Max",
    ///       "lastName": "Mustermann",
    ///       "tenantId": "11111111-1111-1111-1111-111111111111"
    ///     }
    /// 
    /// **Passwort-Anforderungen:**
    /// - Mindestens 8 Zeichen
    /// - Mindestens ein Großbuchstabe
    /// - Mindestens eine Zahl
    /// 
    /// Der neue Benutzer wird mit der Rolle **User** angelegt.
    /// </remarks>
    /// <response code="200">Registrierung erfolgreich</response>
    /// <response code="400">Ungültige Daten oder Email bereits vergeben</response>
    [HttpPost("register")]
    [SwaggerOperation(
        Summary = "Neuen Benutzer registrieren",
        Description = "Erstellt einen neuen Benutzer-Account und gibt JWT Tokens zurück.",
        OperationId = "Register",
        Tags = new[] { "Authentifizierung" }
    )]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var result = await _authService.RegisterAsync(request);
        if (!result.Success)
        {
            return BadRequest(result);
        }
        return Ok(result);
    }

    /// <summary>
    /// Access Token erneuern
    /// </summary>
    /// <param name="refreshToken">Refresh Token vom Login</param>
    /// <returns>Neuer Access Token</returns>
    /// <remarks>
    /// Verwenden Sie den Refresh Token aus der Login-Response,
    /// um einen neuen Access Token zu erhalten, wenn der alte abgelaufen ist.
    /// 
    /// Refresh Tokens sind 7 Tage gültig.
    /// </remarks>
    /// <response code="200">Token erfolgreich erneuert</response>
    /// <response code="401">Ungültiger oder abgelaufener Refresh Token</response>
    [HttpPost("refresh")]
    [SwaggerOperation(
        Summary = "Token erneuern",
        Description = "Generiert einen neuen Access Token mit einem gültigen Refresh Token.",
        OperationId = "RefreshToken",
        Tags = new[] { "Authentifizierung" }
    )]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RefreshToken([FromBody] string refreshToken)
    {
        await _authService.RefreshTokenAsync(refreshToken);
        return Ok();
    }
}
