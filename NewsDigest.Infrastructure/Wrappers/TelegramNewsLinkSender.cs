using Telegram.Bot;
using Telegram.Bot.Types;

namespace NewsDigest.Infrastructure.Wrappers;

public class TelegramNewsLinkSender(ILogger<TelegramNewsLinkSender> logger, ITelegramBotClient botClient) : INewsLinkSender
{
    public async Task SendNewsLinkAsync(long telegramUserId, DigestItem item, CancellationToken ct = default)
    {
        var text = string.IsNullOrEmpty(item.PreviewText)
            ? item.MessageUrl
            : $"{item.MessageUrl}\n\n{item.PreviewText}";

        await botClient.SendMessage(new ChatId(telegramUserId), text, cancellationToken: ct);
        logger.LogInformation("Пользователю {UserId} отправлена новость {MessageUrl}", telegramUserId, item.MessageUrl);
    }
}