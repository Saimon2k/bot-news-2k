using Microsoft.EntityFrameworkCore;
using NewsDigest.Core.Models;

namespace NewsDigest.Infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<UserChannel> UserChannels { get; set; }
        public DbSet<ChannelState> ChannelStates { get; set; }
        public DbSet<CollectedMessage> CollectedMessages { get; set; }
        public DbSet<DigestQueue> DigestQueues { get; set; }
        public DbSet<DigestItem> DigestItems { get; set; }

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
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("datetime()");
            });

            modelBuilder.Entity<DigestItem>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.DigestQueueId);
                entity.HasIndex(e => e.IsSent);
                entity.Property(e => e.OrderIndex).HasDefaultValue(0);
            });
        }
    }
}