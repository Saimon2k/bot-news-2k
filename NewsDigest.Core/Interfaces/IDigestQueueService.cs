using System.Threading.Tasks;
using NewsDigest.Core.Models;

namespace NewsDigest.Core.Interfaces
{
    public interface IDigestQueueService
    {
        Task<DigestItem> GetNextItemAsync(int userId);
        Task MarkAsSentAsync(int itemId);
        Task<DigestQueue> GetCurrentQueueAsync(int userId);
    }
}