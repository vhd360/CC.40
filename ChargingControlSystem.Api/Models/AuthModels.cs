using System.ComponentModel.DataAnnotations;
using Swashbuckle.AspNetCore.Annotations;

namespace ChargingControlSystem.Api.Models;

/// <summary>
/// Login-Anfrage für Benutzer-Authentifizierung
/// </summary>
public class LoginRequest
{
    /// <summary>
    /// E-Mail-Adresse des Benutzers
    /// </summary>
    /// <example>admin@chargingcontrol.com</example>
    [Required(ErrorMessage = "E-Mail ist erforderlich")]
    [EmailAddress(ErrorMessage = "Ungültiges E-Mail-Format")]
    [SwaggerSchema("E-Mail-Adresse des Benutzers")]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Passwort des Benutzers
    /// </summary>
    /// <example>admin123</example>
    [Required(ErrorMessage = "Passwort ist erforderlich")]
    [MinLength(6, ErrorMessage = "Passwort muss mindestens 6 Zeichen lang sein")]
    [SwaggerSchema("Passwort des Benutzers (min. 6 Zeichen)")]
    public string Password { get; set; } = string.Empty;
}

/// <summary>
/// Registrierungs-Anfrage für neue Benutzer
/// </summary>
public class RegisterRequest
{
    /// <summary>
    /// Vorname des Benutzers
    /// </summary>
    /// <example>Max</example>
    [Required(ErrorMessage = "Vorname ist erforderlich")]
    [StringLength(50, ErrorMessage = "Vorname darf maximal 50 Zeichen lang sein")]
    [SwaggerSchema("Vorname des neuen Benutzers")]
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// Nachname des Benutzers
    /// </summary>
    /// <example>Mustermann</example>
    [Required(ErrorMessage = "Nachname ist erforderlich")]
    [StringLength(50, ErrorMessage = "Nachname darf maximal 50 Zeichen lang sein")]
    [SwaggerSchema("Nachname des neuen Benutzers")]
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// E-Mail-Adresse (wird als Login verwendet)
    /// </summary>
    /// <example>max.mustermann@example.com</example>
    [Required(ErrorMessage = "E-Mail ist erforderlich")]
    [EmailAddress(ErrorMessage = "Ungültiges E-Mail-Format")]
    [StringLength(100, ErrorMessage = "E-Mail darf maximal 100 Zeichen lang sein")]
    [SwaggerSchema("E-Mail-Adresse für den Login")]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Passwort (min. 8 Zeichen, 1 Großbuchstabe, 1 Zahl)
    /// </summary>
    /// <example>SecurePassword123!</example>
    [Required(ErrorMessage = "Passwort ist erforderlich")]
    [MinLength(8, ErrorMessage = "Passwort muss mindestens 8 Zeichen lang sein")]
    [SwaggerSchema("Passwort (min. 8 Zeichen, 1 Großbuchstabe, 1 Zahl)")]
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Telefonnummer (optional)
    /// </summary>
    /// <example>+49 123 456789</example>
    [Phone(ErrorMessage = "Ungültiges Telefonnummer-Format")]
    [SwaggerSchema("Telefonnummer des Benutzers (optional)", Nullable = true)]
    public string? PhoneNumber { get; set; }

    /// <summary>
    /// Einladungs-Token für QR-Code-Registrierung (optional)
    /// </summary>
    /// <example>abc123def456</example>
    [SwaggerSchema("QR-Code Einladungs-Token für Gruppenzuordnung (optional)", Nullable = true)]
    public string? InviteToken { get; set; }
}

/// <summary>
/// Authentifizierungs-Antwort mit JWT Token
/// </summary>
public class AuthResponse
{
    /// <summary>
    /// Erfolgs-Status der Authentifizierung
    /// </summary>
    /// <example>true</example>
    [SwaggerSchema("Gibt an, ob die Authentifizierung erfolgreich war")]
    public bool Success { get; set; }

    /// <summary>
    /// JWT Access Token (gültig 24h)
    /// </summary>
    /// <example>eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...</example>
    [SwaggerSchema("JWT Access Token für API-Authentifizierung", Nullable = true)]
    public string? Token { get; set; }

    /// <summary>
    /// Refresh Token zum Erneuern des Access Tokens (gültig 7 Tage)
    /// </summary>
    /// <example>abc123def456ghi789...</example>
    [SwaggerSchema("Refresh Token zum Erneuern des Access Tokens", Nullable = true)]
    public string? RefreshToken { get; set; }

    /// <summary>
    /// Ablaufzeitpunkt des Access Tokens
    /// </summary>
    /// <example>2025-10-24T15:00:00Z</example>
    [SwaggerSchema("UTC-Zeitstempel wann der Access Token abläuft", Nullable = true)]
    public DateTime? ExpiresAt { get; set; }

    /// <summary>
    /// Fehlermeldung bei erfolgloser Authentifizierung
    /// </summary>
    /// <example>Ungültige Anmeldedaten</example>
    [SwaggerSchema("Fehlermeldung falls Success=false", Nullable = true)]
    public string? Message { get; set; }

    /// <summary>
    /// Benutzer-Informationen
    /// </summary>
    [SwaggerSchema("Details des authentifizierten Benutzers", Nullable = true)]
    public UserDto? User { get; set; }

    /// <summary>
    /// ID der Gruppe, der der Benutzer beigetreten ist (bei Einladungs-Registrierung)
    /// </summary>
    /// <example>22222222-2222-2222-2222-222222222222</example>
    [SwaggerSchema("Gruppen-ID falls über Einladung registriert", Nullable = true)]
    public Guid? JoinedGroupId { get; set; }

    /// <summary>
    /// Name der Gruppe, der der Benutzer beigetreten ist
    /// </summary>
    /// <example>Mitarbeiter Gruppe</example>
    [SwaggerSchema("Gruppenname falls über Einladung registriert", Nullable = true)]
    public string? JoinedGroupName { get; set; }
}

/// <summary>
/// Benutzer-Datenmodell
/// </summary>
public class UserDto
{
    /// <summary>
    /// Eindeutige Benutzer-ID
    /// </summary>
    /// <example>388d8e09-5156-4ffd-8ee7-4207eec43c6d</example>
    [SwaggerSchema("Eindeutige Benutzer-ID (GUID)")]
    public Guid Id { get; set; }

    /// <summary>
    /// Tenant-ID des Benutzers
    /// </summary>
    /// <example>11111111-1111-1111-1111-111111111111</example>
    [SwaggerSchema("Tenant-ID des Benutzers")]
    public Guid TenantId { get; set; }

    /// <summary>
    /// Name des Tenants
    /// </summary>
    /// <example>ChargingControl Demo</example>
    [SwaggerSchema("Name der Organisation/Firma")]
    public string TenantName { get; set; } = string.Empty;

    /// <summary>
    /// Logo-URL des Tenants
    /// </summary>
    /// <example>/uploads/tenants/11111111-1111-1111-1111-111111111111/logo.png</example>
    [SwaggerSchema("URL zum Tenant-Logo", Nullable = true)]
    public string? TenantLogoUrl { get; set; }

    /// <summary>
    /// Farbschema des Tenants (0=Blue, 1=Green, 2=Red, 3=Purple)
    /// </summary>
    /// <example>0</example>
    [SwaggerSchema("Farbschema-Index für die Oberfläche")]
    public int TenantTheme { get; set; }

    /// <summary>
    /// Vorname des Benutzers
    /// </summary>
    /// <example>Max</example>
    [SwaggerSchema("Vorname des Benutzers")]
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// Nachname des Benutzers
    /// </summary>
    /// <example>Mustermann</example>
    [SwaggerSchema("Nachname des Benutzers")]
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// E-Mail-Adresse
    /// </summary>
    /// <example>max.mustermann@example.com</example>
    [SwaggerSchema("E-Mail-Adresse des Benutzers")]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Benutzer-Rolle (Admin, Manager, User)
    /// </summary>
    /// <example>User</example>
    [SwaggerSchema("Rolle des Benutzers (Admin, Manager, User)")]
    public string Role { get; set; } = string.Empty;

    /// <summary>
    /// Aktiv-Status des Benutzers
    /// </summary>
    /// <example>true</example>
    [SwaggerSchema("Gibt an, ob der Benutzer aktiv ist")]
    public bool IsActive { get; set; }

    /// <summary>
    /// Erstellungsdatum (UTC)
    /// </summary>
    /// <example>2025-10-01T10:00:00Z</example>
    [SwaggerSchema("UTC-Zeitstempel der Benutzer-Erstellung")]
    public DateTime CreatedAt { get; set; }
}