using Microsoft.EntityFrameworkCore;
using NewsDigest.Infrastructure.Data;

namespace NewsDigest.Infrastructure.Repositories;

public class UserChannelRepository(AppDbContext context) : IUserChannelRepository
{
    public async Task<UserChannel> AddAsync(UserChannel channel, CancellationToken ct = default)
    {
        context.UserChannels.Add(channel);
        await context.SaveChangesAsync(ct);
        return channel;
    }

    public async Task<IReadOnlyList<UserChannel>> GetAllForUserAsync(int userId, CancellationToken ct = default) =>
        await context.UserChannels.AsNoTracking().Where(c => c.UserId == userId).OrderBy(c => c.Title).ToListAsync(ct);

    public async Task<UserChannel?> GetAsync(int id, CancellationToken ct = default) =>
        await context.UserChannels.FindAsync([id, ct]);

    public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
    {
        var channel = await context.UserChannels.FindAsync([id, ct]);
        if (channel is null) return false;

        context.UserChannels.Remove(channel);
        await context.SaveChangesAsync(ct);
        return true;
    }

    public async Task<UserChannel?> GetByChannelIdAsync(int userId, long channelId, CancellationToken ct = default) =>
        await context.UserChannels.FirstOrDefaultAsync(c => c.UserId == userId && c.ChannelId == channelId, ct);
}