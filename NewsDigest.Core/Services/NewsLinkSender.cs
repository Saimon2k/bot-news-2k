using System.Threading.Tasks;
using NewsDigest.Core.Interfaces;
using NewsDigest.Core.Models;
using Serilog;

namespace NewsDigest.Core.Services
{
    public class NewsLinkSender : INewsLinkSender
    {
        private readonly ILogger _logger;
        private readonly IBotService _botService;

        public NewsLinkSender(ILogger logger, IBotService botService)
        {
            _logger = logger;
            _botService = botService;
        }

        public async Task SendNewsLinkAsync(int userId, DigestItem digestItem)
        {
            try
            {
                // Actually send message via Telegram Bot to userId with URL from digestItem.MessageUrl
                _logger.Information("Sending news link to user {UserId}: {MessageUrl}", userId, digestItem.MessageUrl);
                
                // This would actually use botService to send a message to the user
                // Implementation would depend on Telegram.Bot usage
                
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error sending news link to user {UserId}", userId);
                throw;
            }
        }
    }
}