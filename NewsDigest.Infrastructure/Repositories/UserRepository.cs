using Microsoft.EntityFrameworkCore;
using NewsDigest.Infrastructure.Data;

namespace NewsDigest.Infrastructure.Repositories;

public class UserRepository(AppDbContext context) : IUserRepository
{
    public async Task<User?> GetByTelegramIdAsync(long telegramUserId, CancellationToken ct = default) =>
        await context.Users.FirstOrDefaultAsync(u => u.TelegramUserId == telegramUserId, ct);

    public async Task<User> GetOrCreateAsync(long telegramUserId, CancellationToken ct = default)
    {
        var user = await GetByTelegramIdAsync(telegramUserId, ct);
        if (user is not null) return user;

        user = new User { TelegramUserId = telegramUserId };
        context.Users.Add(user);
        await context.SaveChangesAsync(ct);
        return user;
    }
}