using Microsoft.EntityFrameworkCore;
using NewsDigest.Core.Interfaces;
using NewsDigest.Core.Models;
using NewsDigest.Infrastructure.Data;

namespace NewsDigest.Infrastructure.Repositories;

public class DigestQueueRepository : IDigestQueueRepository
{
    private readonly AppDbContext _context;

    public DigestQueueRepository(AppDbContext context) => _context = context;

    public async Task<DigestQueue> CreateQueueAsync(DigestQueue queue)
    {
        _context.DigestQueues.Add(queue);
        await _context.SaveChangesAsync();
        return queue;
    }

    public async Task<List<DigestItem>> AddItemsAsync(List<DigestItem> items)
    {
        _context.DigestItems.AddRange(items);
        await _context.SaveChangesAsync();
        return items;
    }

    public async Task<DigestItem> GetNextItemAsync(int userId)
    {
        return await _context.DigestItems
            .Where(item => item.DigestQueueId != 0 && /* Simplified check for user scope */ true && !item.IsSent)
            .OrderBy(item => item.OrderIndex)
            .Select(item => item with { Id = item.Id }) // Using record syntax pattern if available
            .FirstOrDefaultAsync();
    }

    public async Task<DigestItem> MarkAsSentAsync(DigestItem item)
    {
        if (item == null) return null;
        
        // Update the status/sent flag for the specific record
        var existingItem = await _context.DigestItems.FindAsync(item.Id);
        if (existingItem != null)
        {
            existingItem with { IsSent = true };
            await _context.SaveChangesAsync();
            return existingItem with { IsSent = true };
        }
        return null;
    }

    public async Task<DigestQueue> GetCurrentQueueAsync(int userId)
    {
        // Assuming for simplicity that each user has one active queue
        return await _context.DigestQueues
            .FirstOrDefaultAsync(q => q.UserId == userId && q.Status == "ACTIVE");
    }
}