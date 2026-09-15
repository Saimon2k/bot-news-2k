namespace NewsDigest.Core.Models;

public enum MessageType { Text, MediaWithCaption, MediaWithoutText }

public class CollectedMessage
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public long ChannelId { get; set; }
    public string ChannelUsername { get; set; } = string.Empty;
    public long MessageId { get; set; }
    public DateTime Date { get; set; }
    public string Text { get; set; } = string.Empty;
    public MessageType Type { get; set; }
    public string NormalizedText { get; set; } = string.Empty;
    public string ContentHash { get; set; } = string.Empty;
    public bool IsCanonical { get; set; } = true;
    public int? DuplicateOfMessageId { get; set; }
}