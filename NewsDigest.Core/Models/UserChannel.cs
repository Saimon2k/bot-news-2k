namespace NewsDigest.Core.Models;

public class UserChannel
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public long ChannelId { get; set; }
    public string ChannelUsername { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}