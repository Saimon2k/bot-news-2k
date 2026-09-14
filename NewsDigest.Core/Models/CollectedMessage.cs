namespace NewsDigest.Core.Models;

public record CollectedMessage(int Id, int UserId, long ChannelId, long MessageId, DateTime Date, string Text, MessageType Type, string NormalizedText, string ContentHash, bool IsCanonical, int? DuplicateOfMessageId);

public enum MessageType { Text, MediaWithCaption, MediaWithoutText }