using Microsoft.EntityFrameworkCore;

namespace NewsDigest.Infrastructure.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<UserChannel> UserChannels => Set<UserChannel>();
    public DbSet<ChannelState> ChannelStates => Set<ChannelState>();
    public DbSet<CollectedMessage> CollectedMessages => Set<CollectedMessage>();
    public DbSet<DigestQueue> DigestQueues => Set<DigestQueue>();
    public DbSet<DigestItem> DigestItems => Set<DigestItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.TelegramUserId).IsUnique();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("datetime()");
        });

        modelBuilder.Entity<UserChannel>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.UserId, e.ChannelId }).IsUnique();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("datetime()");
        });

        modelBuilder.Entity<ChannelState>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.UserId, e.ChannelId }).IsUnique();
        });

        modelBuilder.Entity<CollectedMessage>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.ContentHash);
            entity.HasIndex(e => e.DuplicateOfMessageId);
        });

        modelBuilder.Entity<DigestQueue>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.UserId, e.Status });
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("datetime()");
        });

        modelBuilder.Entity<DigestItem>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.DigestQueueId);
            entity.HasIndex(e => new { e.DigestQueueId, e.IsSent });
            entity.Property(e => e.OrderIndex).HasDefaultValue(0);
        });
    }
}