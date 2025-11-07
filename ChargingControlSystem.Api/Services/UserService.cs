using ChargingControlSystem.Data;
using ChargingControlSystem.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace ChargingControlSystem.Api.Services;

public class UserService : IUserService
{
    private readonly ApplicationDbContext _context;
    private readonly ITenantService _tenantService;

    public UserService(ApplicationDbContext context, ITenantService tenantService)
    {
        _context = context;
        _tenantService = tenantService;
    }

    public async Task<User?> GetUserByIdAsync(Guid userId)
    {
        var tenant = await _tenantService.GetCurrentTenantAsync();
        if (tenant == null) return null;

        return await _context.Users
            .FirstOrDefaultAsync(u => u.Id == userId && u.TenantId == tenant.Id);
    }

    public async Task<IEnumerable<User>> GetUsersAsync()
    {
        var tenant = await _tenantService.GetCurrentTenantAsync();
        if (tenant == null) return new List<User>();

        return await _context.Users
            .Where(u => u.TenantId == tenant.Id && u.IsActive)
            .ToListAsync();
    }

    public async Task<User> CreateUserAsync(User user)
    {
        var tenant = await _tenantService.GetCurrentTenantAsync();
        if (tenant != null)
        {
            user.TenantId = tenant.Id;
        }

        user.Id = Guid.NewGuid();
        user.CreatedAt = DateTime.UtcNow;

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return user;
    }

    public async Task UpdateUserAsync(User user)
    {
        _context.Users.Update(user);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteUserAsync(Guid userId)
    {
        var user = await GetUserByIdAsync(userId);
        if (user != null)
        {
            user.IsActive = false;
            await UpdateUserAsync(user);
        }
    }
}
