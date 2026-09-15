namespace NewsDigest.Core.Models;

public class User
{
    public int Id { get; set; }
    public long TelegramUserId { get; set; }
    public DateTime CreatedAt { get; set; }
}