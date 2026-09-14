namespace NewsDigest.Core.Models;

public record ChannelState(int Id, int UserId, long ChannelId, DateTime LastDigestDate, long LastMessageId);