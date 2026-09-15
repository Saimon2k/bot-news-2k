namespace NewsDigest.Core.Interfaces;

public interface IDigestQueueRepository
{
    Task<DigestQueue> CreateQueueAsync(DigestQueue queue, CancellationToken ct = default);
    Task AddItemsAsync(IReadOnlyList<DigestItem> items, CancellationToken ct = default);
    Task<DigestItem?> GetNextItemAsync(int userId, CancellationToken ct = default);
    Task<DigestItem?> GetByIdAsync(int itemId, CancellationToken ct = default);
    Task<int> CountUnsentAsync(int userId, CancellationToken ct = default);
    Task MarkAsSentAsync(int itemId, CancellationToken ct = default);
    Task<DigestQueue?> GetCurrentQueueAsync(int userId, CancellationToken ct = default);
}