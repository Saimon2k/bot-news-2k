using System.Threading.Tasks;
using System.Collections.Generic;
using NewsDigest.Core.Models;

namespace NewsDigest.Core.Interfaces
{
    public interface IUserChannelRepository
    {
        Task<UserChannel> AddAsync(UserChannel channel);
        Task<List<UserChannel>> GetAllForUserAsync(int userId);
        Task<UserChannel> GetAsync(int id);
        Task DeleteAsync(int id);
    }
}