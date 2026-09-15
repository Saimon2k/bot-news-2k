namespace NewsDigest.Core.Interfaces;

public interface INewsLinkSender
{
    Task SendNewsLinkAsync(long telegramUserId, DigestItem digestItem, CancellationToken ct = default);
}