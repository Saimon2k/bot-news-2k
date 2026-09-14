using Microsoft.EntityFrameworkCore;
using NewsDigest.Core.Interfaces;
using NewsDigest.Core.Models;

namespace NewsDigest.Infrastructure.Repositories;

public class UserChannelRepository : IUserChannelRepository
{
    private readonly AppDbContext _context;

    // Primary constructor style
    public UserChannelRepository(AppDbContext context) => _context = context;

    public async Task<UserChannel> AddAsync(UserChannel channel)
    {
        _context.UserChannels.Add(channel);
        await _context.SaveChangesAsync();
        return channel;
    }

    public async Task<List<UserChannel>> GetAllForUserAsync(int userId) => 
        await _context.UserChannels.Where(c => c.UserId == userId).ToListAsync();

    public async Task<UserChannel> GetAsync(int id) => 
        await _context.UserChannels.FindAsync(id);

    public async Task DeleteAsync(int id)
    {
        var channel = await _context.UserChannels.FindAsync(id);
        if (channel != null)
        {
            _context.UserChannels.Remove(channel);
            await _context.SaveChangesAsync();
        }
    }
}