namespace NewsDigest.Core.Models;

public static class DigestQueueStatus
{
    public const string Active = "ACTIVE";
    public const string Finished = "FINISHED";
}

public class DigestQueue
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public DateTime CreatedAt { get; set; }
    public string Status { get; set; } = DigestQueueStatus.Active;
    public string SourceBatchPath { get; set; } = string.Empty;
}