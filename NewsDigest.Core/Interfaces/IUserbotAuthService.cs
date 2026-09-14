using System.Threading.Tasks;

namespace NewsDigest.Core.Interfaces
{
    public interface IUserbotAuthService
    {
        Task<bool> LoginAsync();
    }
}