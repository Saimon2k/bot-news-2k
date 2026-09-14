using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using NewsDigest.Core.Models;

namespace NewsDigest.Core.Interfaces
{
    public interface INewsCollector
    {
        Task<List<CollectedMessage>> CollectNewsAsync(int userId);
    }
}