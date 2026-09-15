namespace NewsDigest.Core.Models;

public sealed record NewsCollectionResult(
    IReadOnlyList<CollectedMessage> Messages,
    string BatchPath,
    IReadOnlyList<long> ProcessedChannelIds);