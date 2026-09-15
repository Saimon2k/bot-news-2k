namespace NewsDigest.Infrastructure.Wrappers;

public class UserbotAuthService(ILogger<UserbotAuthService> logger, IUserbotClient userbotClient) : IUserbotAuthService
{
    public async Task<bool> LoginAsync(CancellationToken ct = default)
    {
        try
        {
            var ok = await userbotClient.ConnectAsync(ct);
            if (ok) logger.LogInformation("Юзербот успешно авторизован");
            return ok;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ошибка авторизации юзербота");
            return false;
        }
    }
}