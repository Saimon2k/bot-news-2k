using System.Threading.Tasks;
using System.Collections.Generic;

namespace NewsDigest.Core.Interfaces
{
    public interface IOllamaClient
    {
        Task<string> GenerateAsync(string prompt);
    }
}