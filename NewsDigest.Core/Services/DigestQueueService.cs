namespace NewsDigest.Core.Services;

public class DigestQueueService(
    ILogger<DigestQueueService> logger,
    IDigestQueueRepository queueRepository) : IDigestQueueService
{
    public Task<DigestQueue?> GetCurrentQueueAsync(int userId, CancellationToken ct = default) =>
        queueRepository.GetCurrentQueueAsync(userId, ct);

    public async Task<DigestItem?> GetNextItemAsync(int userId, CancellationToken ct = default)
    {
        var item = await queueRepository.GetNextItemAsync(userId, ct);
        if (item is null) logger.LogInformation("Очередь пользователя {UserId} закончилась", userId);
        return item;
    }

    public async Task MarkAsSentAsync(int itemId, CancellationToken ct = default) =>
        await queueRepository.MarkAsSentAsync(itemId, ct);
}