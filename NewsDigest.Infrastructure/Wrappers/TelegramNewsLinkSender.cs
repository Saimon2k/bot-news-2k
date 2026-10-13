using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace NewsDigest.Infrastructure.Wrappers;

public class TelegramNewsLinkSender(ILogger<TelegramNewsLinkSender> logger, ITelegramBotClient botClient) : INewsLinkSender
{
    public async Task SendNewsLinkAsync(long telegramUserId, DigestItem item, CancellationToken ct = default)
    {
        try
        {
            // Формируем красивое сообщение с превью и ссылкой
            // Используем HTML для форматирования
            var message = $"📰 <b>{EscapeHtml(item.ChannelUsername.TrimStart('@'))}</b>\n\n" +
                         $"{EscapeHtml(item.PreviewText)}\n\n" +
                         $"<a href=\"{item.MessageUrl}\">Читать полностью →</a>";
            
            await botClient.SendMessage(
                chatId: new ChatId(telegramUserId),
                text: message,
                parseMode: ParseMode.Html,
                linkPreviewOptions: new Telegram.Bot.Types.LinkPreviewOptions { IsDisabled = false },
                cancellationToken: ct
            );
            
            logger.LogInformation("Пользователю {UserId} отправлена новость {MessageId} из канала {ChannelId}", 
                telegramUserId, item.MessageId, item.ChannelId);
        }
        catch (ApiRequestException ex) when (ex.Message.Contains("chat not found"))
        {
            // Пользователь не начал диалог с ботом — бот не может отправлять ему сообщения
            // Telegram Bot API не позволяет боту инициировать диалог первым
            logger.LogWarning(
                "Пользователь {UserId} не начал диалог с ботом. Невозможно отправить новость {MessageId}. " +
                "Пользователь должен нажать /start в боте.", 
                telegramUserId, item.MessageId);
            throw new InvalidOperationException(
                $"Пользователь {telegramUserId} не начал диалог с ботом. Отправка невозможна.", ex);
        }
        catch (ApiRequestException ex) when (ex.Message.Contains("bot was blocked"))
        {
            // Пользователь заблокировал бота
            logger.LogWarning("Пользователь {UserId} заблокировал бота. Невозможно отправить новость {MessageId}.", 
                telegramUserId, item.MessageId);
            throw new InvalidOperationException(
                $"Пользователь {telegramUserId} заблокировал бота. Отправка невозможна.", ex);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ошибка при отправке новости {MessageId} из канала {ChannelId} пользователю {UserId}", 
                item.MessageId, item.ChannelId, telegramUserId);
            throw;
        }
    }

    private static string EscapeHtml(string text)
    {
        return text
            .Replace("&", "&amp;")
            .Replace("<", "&lt;")
            .Replace(">", "&gt;")
            .Replace("\"", "&quot;");
    }
}