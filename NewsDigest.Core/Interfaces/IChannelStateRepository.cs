using System.Threading.Tasks;
using NewsDigest.Core.Models;

namespace NewsDigest.Core.Interfaces
{
    public interface IChannelStateRepository
    {
        Task<ChannelState> GetAsync(int userId, long channelId);
        Task<ChannelState> UpdateAsync(ChannelState state);
    }
}