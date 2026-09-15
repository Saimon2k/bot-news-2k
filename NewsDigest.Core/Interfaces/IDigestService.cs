namespace NewsDigest.Core.Interfaces;

public interface IDigestService
{
    Task<IReadOnlyList<DigestItem>> CreateDigestAsync(int userId, IReadOnlyList<CollectedMessage> messages, CancellationToken ct = default);
}