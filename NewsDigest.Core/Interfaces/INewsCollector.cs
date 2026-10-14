namespace NewsDigest.Core.Interfaces;

public interface INewsCollector
{
    Task<NewsCollectionResult> CollectNewsAsync(int userId, CancellationToken ct = default, TimeSpan? lookbackPeriod = null);
}