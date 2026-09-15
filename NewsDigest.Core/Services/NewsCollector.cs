using System.Text.Json;

namespace NewsDigest.Core.Services;

public class NewsCollector(
    ILogger<NewsCollector> logger,
    IUserbotClient userbotClient,
    IUserChannelRepository userChannelRepository,
    IChannelStateRepository channelStateRepository,
    IOptions<StorageOptions> storageOptions) : INewsCollector
{
    public async Task<NewsCollectionResult> CollectNewsAsync(int userId, CancellationToken ct = default)
    {
        var channels = await userChannelRepository.GetAllForUserAsync(userId, ct);
        if (channels.Count == 0) return new NewsCollectionResult([], string.Empty, []);

        var collected = new List<CollectedMessage>();
        var processedChannelIds = new List<long>();

        foreach (var channel in channels)
        {
            ct.ThrowIfCancellationRequested();
            var state = await channelStateRepository.GetOrCreateAsync(userId, channel.ChannelId, ct);

            IReadOnlyList<ChannelMessage> history;
            try
            {
                history = await userbotClient.GetHistoryAsync(channel.ChannelId, channel.ChannelUsername, state.LastDigestDate, DateTime.UtcNow, ct);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Ошибка сбора истории канала {ChannelId} ({Username})", channel.ChannelId, channel.ChannelUsername);
                continue;
            }

            var before = collected.Count;
            foreach (var msg in history)
            {
                if (string.IsNullOrWhiteSpace(msg.Text) && !msg.HasMedia) continue;

                var normalized = NewsNormalizer.Normalize(msg.Text);
                collected.Add(new CollectedMessage
                {
                    UserId = userId,
                    ChannelId = msg.ChannelId,
                    ChannelUsername = msg.ChannelUsername ?? channel.ChannelUsername,
                    MessageId = msg.MessageId,
                    Date = msg.Date,
                    Text = msg.Text,
                    Type = msg.Classify(),
                    NormalizedText = normalized,
                    ContentHash = NewsNormalizer.ComputeHash(normalized),
                });
            }

            processedChannelIds.Add(channel.ChannelId);
            logger.LogInformation("Канал {Username}: собрано {Count} сообщений", channel.ChannelUsername, collected.Count - before);
        }

        for (var i = 0; i < collected.Count; i++) collected[i].Id = i + 1;
        NewsNormalizer.SelectCanonicals(collected);

        var canonical = collected.Where(m => m.IsCanonical).ToList();
        var batchPath = await SaveBatchAsync(userId, collected, ct);

        logger.LogInformation("Сбор завершён: {Total} сообщений, {Canonical} канонических", collected.Count, canonical.Count);
        return new NewsCollectionResult(canonical, batchPath, processedChannelIds);
    }

    private async Task<string> SaveBatchAsync(int userId, IReadOnlyList<CollectedMessage> batch, CancellationToken ct)
    {
        var directory = Path.Combine(storageOptions.Value.BatchDirectory, userId.ToString());
        Directory.CreateDirectory(directory);

        var path = Path.Combine(directory, $"{DateTime.Now:yyyyMMddHHmmss}.json");
        await using var stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None, 4096, FileOptions.Asynchronous);
        await JsonSerializer.SerializeAsync(stream, batch, (JsonSerializerOptions?)null, ct);
        return path;
    }
}