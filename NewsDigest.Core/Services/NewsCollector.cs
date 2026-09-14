namespace NewsDigest.Core.Services;

public class NewsCollector : INewsCollector
{
    private readonly IUserbotClient _userbotClient;
    private readonly IChannelStateRepository _channelStateRepository;

    // Primary constructor style
    public NewsCollector(IUserbotClient userbotClient, IChannelStateRepository channelStateRepository) => (_userbotClient, _channelStateRepository) = (userbotClient, channelStateRepository);

    public async Task<List<CollectedMessage>> CollectNewsAsync(int userId) {
        // Simplified implementation placeholder.
        // Actual logic to call WTelegram, filter by last_date, classify, and hash goes here.
        return new List<CollectedMessage>();
    }
}