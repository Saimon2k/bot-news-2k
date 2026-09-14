using System.Threading.Tasks;
using System.Collections.Generic;
using Telegram.Bot.Types;

namespace NewsDigest.Core.Interfaces
{
    public interface IUserbotClient
    {
        Task<bool> ConnectAsync();
        Task<List<Message>> GetHistoryAsync(long channelId, DateTime from, DateTime to);
        Task<string> GetMessageUrlAsync(long channelId, long messageId);
    }
}