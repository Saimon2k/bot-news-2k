using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace NewsDigest.Core.Interfaces
{
    public interface IBotService
    {
        Task HandleCommandAsync(Update update);
    }
}