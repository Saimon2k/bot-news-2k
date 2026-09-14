using System.Threading.Tasks;
using NewsDigest.Core.Interfaces;
using NewsDigest.Core.Models;

namespace NewsDigest.Core.Services
{
    public class DigestQueueService : IDigestQueueService
    {
        private readonly IDigestQueueRepository _queueRepository;

        public DigestQueueService(IDigestQueueRepository queueRepository)
        {
            _queueRepository = queueRepository;
        }

        public async Task<DigestItem> GetNextItemAsync(int userId)
        {
            return await _queueRepository.GetNextItemAsync(userId);
        }

        public async Task MarkAsSentAsync(int itemId)
        {
            var item = await _queueRepository.GetNextItemAsync(itemId);
            if (item != null)
            {
                await _queueRepository.MarkAsSentAsync(item);
            }
        }

        public async Task<DigestQueue> GetCurrentQueueAsync(int userId)
        {
            return await _queueRepository.GetCurrentQueueAsync(userId);
        }
    }
}