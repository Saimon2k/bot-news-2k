namespace NewsDigest.Core.Interfaces;

public interface IUserbotAuthService
{
    Task<bool> LoginAsync(CancellationToken ct = default);
}