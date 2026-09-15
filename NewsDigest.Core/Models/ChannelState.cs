namespace NewsDigest.Core.Models;

public class ChannelState
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public long ChannelId { get; set; }
    public DateTime LastDigestDate { get; set; }
    public long LastMessageId { get; set; }
}