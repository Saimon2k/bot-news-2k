using System.Threading.Tasks;
using NewsDigest.Core.Models;

namespace NewsDigest.Core.Interfaces
{
    public interface INewsLinkSender
    {
        Task SendNewsLinkAsync(int userId, DigestItem digestItem);
    }
}