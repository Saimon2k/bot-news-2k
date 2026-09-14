using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using WTelegram;

namespace NewsDigest.Infrastructure.Wrappers;

// Using record for immutability and simplicity
public class UserbotClient : IUserbotClient
{
    private readonly ILogger _logger;
    private readonly string _sessionPath;
    private readonly string _apiId;
    private readonly string _apiHash;
    private readonly string _phoneNumber;

    // Primary constructor style
    public UserbotClient(ILogger logger, string sessionPath, string apiId, string apiHash, string phoneNumber)
    {
        _logger = logger;
        _sessionPath = sessionPath;
        _apiId = apiId;
        _apiHash = apiHash;
        _phoneNumber = phoneNumber;
    }

    public async Task<bool> ConnectAsync()
    {
        if (_client != null) return true;

        try
        {
            _logger.Information("Attempting to connect to Telegram API...");
            _client = new Client(_apiId, _apiHash);
            
            var connected = await _client.ConnectAsync();
            
            if (!connected)
            {
                _logger.Warning("Failed to establish connection to Telegram API");
                return false;
            }
            
            _logger.Information("Successfully connected to Telegram API");
            return true;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error while connecting to Telegram API");
            return false;
        }
    }

    // Simplified version for refactoring example
    public async Task<List<Message>> GetHistoryAsync(long channelId, DateTime from, DateTime to)
    {
        if (_client == null) throw new InvalidOperationException("Client is not connected");
        
        _logger.Information("Fetching history for {ChannelId} from {FromDate:yyyy-MM-dd} to {ToDate:yyyy-MM-dd}", channelId, from: from, to: to);
        
        // Placeholder implementation
        return new List<Message>(); 
    }

    public Task<string> GetMessageUrlAsync(long channelId, long messageId) => 
        Task.FromResult($"t.me/{channelId}/{messageId}");
}