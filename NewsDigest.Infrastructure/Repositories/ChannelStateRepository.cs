using Microsoft.EntityFrameworkCore;
using NewsDigest.Infrastructure.Data;

namespace NewsDigest.Infrastructure.Repositories;

public class ChannelStateRepository(AppDbContext context) : IChannelStateRepository
{
    public async Task<ChannelState?> GetAsync(int userId, long channelId, CancellationToken ct = default) =>
        await context.ChannelStates.AsNoTracking().FirstOrDefaultAsync(s => s.UserId == userId && s.ChannelId == channelId, ct);

    public async Task<ChannelState> GetOrCreateAsync(int userId, long channelId, CancellationToken ct = default)
    {
        var state = await GetAsync(userId, channelId, ct);
        if (state is not null) return state;

        state = new ChannelState { UserId = userId, ChannelId = channelId };
        context.ChannelStates.Add(state);
        await context.SaveChangesAsync(ct);
        return state;
    }

    public async Task UpdateLastDigestAsync(int userId, long channelId, DateTime lastDigestDate, long lastMessageId, CancellationToken ct = default)
    {
        var state = await context.ChannelStates.FirstOrDefaultAsync(s => s.UserId == userId && s.ChannelId == channelId, ct);
        if (state is null)
        {
            context.ChannelStates.Add(new ChannelState
            {
                UserId = userId,
                ChannelId = channelId,
                LastDigestDate = lastDigestDate,
                LastMessageId = lastMessageId,
            });
        }
        else
        {
            state.LastDigestDate = lastDigestDate;
            state.LastMessageId = lastMessageId;
        }

        await context.SaveChangesAsync(ct);
    }
}