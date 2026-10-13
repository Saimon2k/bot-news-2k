using Microsoft.EntityFrameworkCore;
using NewsDigest.Infrastructure.Data;

namespace NewsDigest.Infrastructure.Repositories;

public class DigestQueueRepository(AppDbContext context) : IDigestQueueRepository
{
    public async Task<DigestQueue> CreateQueueAsync(DigestQueue queue, CancellationToken ct = default)
    {
        var activeQueues = await context.DigestQueues
            .Where(q => q.UserId == queue.UserId && q.Status == DigestQueueStatus.Active)
            .ToListAsync(ct);
        foreach (var active in activeQueues) active.Status = DigestQueueStatus.Finished;

        context.DigestQueues.Add(queue);
        await context.SaveChangesAsync(ct);
        return queue;
    }

    public async Task AddItemsAsync(IReadOnlyList<DigestItem> items, CancellationToken ct = default)
    {
        context.DigestItems.AddRange(items);
        await context.SaveChangesAsync(ct);
    }

    public async Task<DigestItem?> GetNextItemAsync(int userId, CancellationToken ct = default)
    {
        var activeQueue = await context.DigestQueues.FirstOrDefaultAsync(q => q.UserId == userId && q.Status == DigestQueueStatus.Active, ct);
        if (activeQueue is null) return null;

        var item = await context.DigestItems
            .Where(i => i.DigestQueueId == activeQueue.Id && !i.IsSent)
            .OrderBy(i => i.OrderIndex)
            .FirstOrDefaultAsync(ct);

        if (item is null)
        {
            activeQueue.Status = DigestQueueStatus.Finished;
            await context.SaveChangesAsync(ct);
            return null;
        }

        return item;
    }

    public async Task<DigestItem?> GetByIdAsync(int itemId, CancellationToken ct = default) =>
        await context.DigestItems.FindAsync([itemId, ct]);

    public async Task<int> CountUnsentAsync(int userId, CancellationToken ct = default)
    {
        var activeQueue = await context.DigestQueues.FirstOrDefaultAsync(q => q.UserId == userId && q.Status == DigestQueueStatus.Active, ct);
        if (activeQueue is null) return 0;

        return await context.DigestItems.CountAsync(i => i.DigestQueueId == activeQueue.Id && !i.IsSent, ct);
    }

    public async Task MarkAsSentAsync(int itemId, CancellationToken ct = default)
    {
        var item = await context.DigestItems.FindAsync([itemId, ct]);
        if (item is null) return;

        item.IsSent = true;
        await context.SaveChangesAsync(ct);
    }

    public async Task<DigestQueue?> GetCurrentQueueAsync(int userId, CancellationToken ct = default) =>
        await context.DigestQueues.FirstOrDefaultAsync(q => q.UserId == userId && q.Status == DigestQueueStatus.Active, ct);
}