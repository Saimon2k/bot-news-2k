using Serilog;
using Telegram.Bot.Types;
using NewsDigest.Core.Interfaces;

namespace NewsDigest.Infrastructure.Wrappers;

public class UserbotAuthService : IUserbotAuthService
{
    private readonly ILogger _logger;
    private readonly IUserbotClient _userbotClient;

    public UserbotAuthService(ILogger logger, IUserbotClient userbotClient) => (_logger, _userbotClient) = (logger, userbotClient);

    public async Task<bool> LoginAsync()
    {
        try
        {
            _logger.Information("Starting userbot authentication process");
            
            if (!(await _userbotClient.ConnectAsync()))
            {
                _logger.Error("Cannot connect to Telegram API for authentication");
                return false;
            }
            
            _logger.Information("Userbot authentication successful");
            return true;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error during userbot authentication");
            return false;
        }
    }
}