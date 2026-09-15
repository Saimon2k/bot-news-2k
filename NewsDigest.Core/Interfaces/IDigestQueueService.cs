namespace NewsDigest.Core.Interfaces;

public interface IDigestQueueService
{
    Task<DigestItem?> GetNextItemAsync(int userId, CancellationToken ct = default);
    Task MarkAsSentAsync(int itemId, CancellationToken ct = default);
    Task<DigestQueue?> GetCurrentQueueAsync(int userId, CancellationToken ct = default);
}