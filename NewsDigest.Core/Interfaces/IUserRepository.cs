namespace NewsDigest.Core.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByTelegramIdAsync(long telegramUserId, CancellationToken ct = default);
    Task<User> GetOrCreateAsync(long telegramUserId, CancellationToken ct = default);
}