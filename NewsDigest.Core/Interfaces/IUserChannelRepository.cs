namespace NewsDigest.Core.Interfaces;

public interface IUserChannelRepository
{
    Task<UserChannel> AddAsync(UserChannel channel, CancellationToken ct = default);
    Task<IReadOnlyList<UserChannel>> GetAllForUserAsync(int userId, CancellationToken ct = default);
    Task<UserChannel?> GetAsync(int id, CancellationToken ct = default);
    Task<bool> DeleteAsync(int id, CancellationToken ct = default);
    Task<UserChannel?> GetByChannelIdAsync(int userId, long channelId, CancellationToken ct = default);
}