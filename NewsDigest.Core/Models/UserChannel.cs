namespace NewsDigest.Core.Models;

public record UserChannel(int Id, int UserId, long ChannelId, string ChannelUsername, string Title, DateTime CreatedAt);