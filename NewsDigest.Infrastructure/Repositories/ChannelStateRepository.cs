using Microsoft.EntityFrameworkCore;
using NewsDigest.Core.Interfaces;
using NewsDigest.Core.Models;
using NewsDigest.Infrastructure.Data;

namespace NewsDigest.Infrastructure.Repositories;

public class ChannelStateRepository : IChannelStateRepository
{
    private readonly AppDbContext _context;

    public ChannelStateRepository(AppDbContext context) => _context = context;

    public async Task<ChannelState> GetAsync(int userId, long channelId) => 
        await _context.ChannelStates.FirstOrDefaultAsync(c => c.UserId == userId && c.ChannelId == channelId);

    public async Task<ChannelState> UpdateAsync(ChannelState state)
    {
        var existing = await _context.ChannelStates.FindAsync(state.Id);
        if (existing != null)
        {
            existing with { LastDigestDate = state.LastDigestDate, LastMessageId = state.LastMessageId };
        }
        await _context.SaveChangesAsync();
        return state;
    }
}