using ChargingControlSystem.Api.Models;
using ChargingControlSystem.Data;
using ChargingControlSystem.Data.Entities;
using ChargingControlSystem.Data.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace ChargingControlSystem.Api.Services;

public class AuthService : IAuthService
{
    private readonly ApplicationDbContext _context;
    private readonly ITenantService _tenantService;
    private readonly IConfiguration _configuration;

    public AuthService(ApplicationDbContext context, ITenantService tenantService, IConfiguration configuration)
    {
        _context = context;
        _tenantService = tenantService;
        _configuration = configuration;
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
{
    // Finde Benutzer NUR anhand der E-Mail (ohne Tenant-Filter)
    var user = await _context.Users
        .Include(u => u.Tenant)  // Lade Tenant mit
        .FirstOrDefaultAsync(u => u.Email == request.Email && u.IsActive);

    if (user == null || !VerifyPassword(request.Password, user.PasswordHash))
    {
        return new AuthResponse { Success = false, Message = "Invalid credentials" };
    }

    // Prüfe, ob der Tenant aktiv ist
    if (!user.Tenant.IsActive)
    {
        return new AuthResponse { Success = false, Message = "Tenant is not active" };
    }

    var token = GenerateJwtToken(user);
    var refreshToken = GenerateRefreshToken();

    // Speichere RefreshToken im User
    user.RefreshToken = refreshToken;
    user.RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(7); // RefreshToken gültig für 7 Tage
    user.LastLoginAt = DateTime.UtcNow;
    await _context.SaveChangesAsync();

    return new AuthResponse
    {
        Success = true,
        Token = token,
        RefreshToken = refreshToken,
        ExpiresAt = DateTime.UtcNow.AddMinutes(int.Parse(_configuration["Jwt:ExpiryInMinutes"]!)),
        User = new UserDto
        {
            Id = user.Id,
            TenantId = user.TenantId,
            TenantName = user.Tenant.Name,
            TenantLogoUrl = user.Tenant.LogoUrl,
            TenantTheme = (int)user.Tenant.Theme,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            Role = user.Role.ToString(),
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt
        }
    };
}
    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        UserGroup? invitedGroup = null;
        
        // If invite token is provided, validate it (but don't use it for tenant assignment yet)
        if (!string.IsNullOrEmpty(request.InviteToken))
        {
            invitedGroup = await _context.UserGroups
                .Include(g => g.Tenant)
                .FirstOrDefaultAsync(g => g.InviteToken == request.InviteToken && 
                                         g.IsActive &&
                                         (!g.InviteTokenExpiresAt.HasValue || g.InviteTokenExpiresAt.Value > DateTime.UtcNow));
            
            if (invitedGroup == null)
            {
                return new AuthResponse { Success = false, Message = "Invalid or expired invite token" };
            }
        }
        
        // User's home tenant: Use default tenant (ChargingControl GmbH)
        // This is the user's "home" tenant, but they can access resources across tenants via group memberships
        var tenant = await _context.Tenants
            .FirstOrDefaultAsync(t => t.Subdomain == "chargingcontrol" && t.IsActive);
            
        if (tenant == null)
        {
            return new AuthResponse { Success = false, Message = "Default tenant not found. Please contact support." };
        }

        // Check if user already exists
        var existingUser = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == request.Email);

        if (existingUser != null)
        {
            return new AuthResponse { Success = false, Message = "User already exists" };
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            TenantId = tenant.Id,
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            PhoneNumber = request.PhoneNumber,
            PasswordHash = HashPassword(request.Password),
            Role = UserRole.User, // Registrierte Benutzer sind standardmäßig normale User
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // If user was invited to a group, automatically add them
        if (invitedGroup != null)
        {
            var membership = new UserGroupMembership
            {
                Id = Guid.NewGuid(),
                UserGroupId = invitedGroup.Id,
                UserId = user.Id,
                AssignedAt = DateTime.UtcNow
            };
            
            _context.UserGroupMemberships.Add(membership);
            await _context.SaveChangesAsync();
        }

        var token = GenerateJwtToken(user);
        var refreshToken = GenerateRefreshToken();

        return new AuthResponse
        {
            Success = true,
            Token = token,
            RefreshToken = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddMinutes(int.Parse(_configuration["Jwt:ExpiryInMinutes"]!)),
            User = new UserDto
            {
                Id = user.Id,
                TenantId = user.TenantId,
                TenantName = tenant.Name,
                TenantLogoUrl = tenant.LogoUrl,
                TenantTheme = (int)tenant.Theme,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Role = user.Role.ToString(),
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt
            },
            JoinedGroupId = invitedGroup?.Id,
            JoinedGroupName = invitedGroup?.Name
        };
    }

    public async Task<bool> ValidateTokenAsync(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Secret"]!);

            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidIssuer = _configuration["Jwt:Issuer"],
                ValidAudience = _configuration["Jwt:Audience"],
                ClockSkew = TimeSpan.Zero
            }, out _);

            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<AuthResponse> RefreshTokenAsync(string refreshToken)
    {
        // Finde User mit gültigem RefreshToken
        var user = await _context.Users
            .Include(u => u.Tenant)
            .FirstOrDefaultAsync(u => u.RefreshToken == refreshToken 
                && u.RefreshTokenExpiresAt.HasValue 
                && u.RefreshTokenExpiresAt.Value > DateTime.UtcNow
                && u.IsActive);

        if (user == null || !user.Tenant.IsActive)
        {
            return new AuthResponse 
            { 
                Success = false, 
                Message = "Invalid or expired refresh token" 
            };
        }

        // Generiere neuen Access Token
        var newToken = GenerateJwtToken(user);
        var newRefreshToken = GenerateRefreshToken();

        // Aktualisiere RefreshToken
        user.RefreshToken = newRefreshToken;
        user.RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(7);
        await _context.SaveChangesAsync();

        return new AuthResponse
        {
            Success = true,
            Token = newToken,
            RefreshToken = newRefreshToken,
            ExpiresAt = DateTime.UtcNow.AddMinutes(int.Parse(_configuration["Jwt:ExpiryInMinutes"]!)),
            User = new UserDto
            {
                Id = user.Id,
                TenantId = user.TenantId,
                TenantName = user.Tenant.Name,
                TenantLogoUrl = user.Tenant.LogoUrl,
                TenantTheme = (int)user.Tenant.Theme,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Role = user.Role.ToString(),
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt
            }
        };
    }

    private string GenerateJwtToken(User user)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, $"{user.FirstName} {user.LastName}"),
            new Claim(ClaimTypes.Role, user.Role.ToString()),
            new Claim("TenantId", user.TenantId.ToString()),
            new Claim("Role", user.Role.ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Secret"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(int.Parse(_configuration["Jwt:ExpiryInMinutes"]!)),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private string GenerateRefreshToken()
    {
        var randomBytes = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }

    private string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password);
    }

    private bool VerifyPassword(string password, string hash)
    {
        return BCrypt.Net.BCrypt.Verify(password, hash);
    }
}

        };
    }

    private string GenerateJwtToken(User user)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, $"{user.FirstName} {user.LastName}"),
            new Claim(ClaimTypes.Role, user.Role.ToString()),
            new Claim("TenantId", user.TenantId.ToString()),
            new Claim("Role", user.Role.ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Secret"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(int.Parse(_configuration["Jwt:ExpiryInMinutes"]!)),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private string GenerateRefreshToken()
    {
        var randomBytes = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }

    private string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password);
    }

    private bool VerifyPassword(string password, string hash)
    {
        return BCrypt.Net.BCrypt.Verify(password, hash);
    }
}
