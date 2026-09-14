namespace NewsDigest.Core.Models;

public record DigestItem(int Id, int DigestQueueId, int OrderIndex, long ChannelId, string ChannelUsername, long MessageId, string MessageUrl, MessageType Type, string PreviewText, string Reason, bool IsSent);