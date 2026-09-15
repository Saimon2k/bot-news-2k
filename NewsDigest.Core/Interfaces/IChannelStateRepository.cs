namespace NewsDigest.Core.Interfaces;

public interface IChannelStateRepository
{
    Task<ChannelState?> GetAsync(int userId, long channelId, CancellationToken ct = default);
    Task<ChannelState> GetOrCreateAsync(int userId, long channelId, CancellationToken ct = default);
    Task UpdateLastDigestAsync(int userId, long channelId, DateTime lastDigestDate, long lastMessageId, CancellationToken ct = default);
}