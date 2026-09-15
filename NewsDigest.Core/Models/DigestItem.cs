namespace NewsDigest.Core.Models;

public class DigestItem
{
    public int Id { get; set; }
    public int DigestQueueId { get; set; }
    public int OrderIndex { get; set; }
    public long ChannelId { get; set; }
    public string ChannelUsername { get; set; } = string.Empty;
    public long MessageId { get; set; }
    public string MessageUrl { get; set; } = string.Empty;
    public MessageType Type { get; set; }
    public string PreviewText { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public bool IsSent { get; set; }
}