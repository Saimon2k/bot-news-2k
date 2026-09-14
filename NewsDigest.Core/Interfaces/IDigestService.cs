using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using NewsDigest.Core.Models;

namespace NewsDigest.Core.Interfaces
{
    public interface IDigestService
    {
        Task<List<DigestItem>> CreateDigestAsync(int userId, List<CollectedMessage> messages);
    }
}