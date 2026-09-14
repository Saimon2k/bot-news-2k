namespace NewsDigest.Core.Models;

public record DigestQueue(int Id, int UserId, DateTime CreatedAt, string Status, string SourceBatchPath);