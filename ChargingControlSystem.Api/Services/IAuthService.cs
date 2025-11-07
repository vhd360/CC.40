using ChargingControlSystem.Api.Models;

namespace ChargingControlSystem.Api.Services;

public interface IAuthService
{
    Task<AuthResponse> LoginAsync(LoginRequest request);
    Task<AuthResponse> RegisterAsync(RegisterRequest request);
    Task<bool> ValidateTokenAsync(string token);
    Task RefreshTokenAsync(string refreshToken);
}
