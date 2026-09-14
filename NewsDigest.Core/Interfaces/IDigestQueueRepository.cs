using System.Threading.Tasks;
using System.Collections.Generic;
using NewsDigest.Core.Models;

namespace NewsDigest.Core.Interfaces
{
    public interface IDigestQueueRepository
    {
        Task<DigestQueue> CreateQueueAsync(DigestQueue queue);
        Task<List<DigestItem>> AddItemsAsync(List<DigestItem> items);
        Task<DigestItem> GetNextItemAsync(int userId);
        Task<DigestItem> MarkAsSentAsync(DigestItem item);
        Task<DigestQueue> GetCurrentQueueAsync(int userId);
    }
}