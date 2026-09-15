namespace NewsDigest.Core.Interfaces;

public interface IUserbotClient
{
    Task<bool> ConnectAsync(CancellationToken ct = default);
    Task<IReadOnlyList<ChannelMessage>> GetHistoryAsync(long channelId, string? channelUsername, DateTime from, DateTime to, CancellationToken ct = default);
    Task<string> GetMessageUrlAsync(long channelId, string? channelUsername, long messageId, CancellationToken ct = default);
    Task<long?> ResolveChannelAsync(string link, CancellationToken ct = default);
}