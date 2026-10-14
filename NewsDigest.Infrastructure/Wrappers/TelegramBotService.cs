using System.Globalization;
using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot;

namespace NewsDigest.Infrastructure.Wrappers;

public class TelegramBotService(
    ILogger<TelegramBotService> logger,
    ITelegramBotClient botClient,
    IServiceScopeFactory scopeFactory,
    INewsLinkSender newsLinkSender) : IBotService
{
    public async Task HandleCommandAsync(BotCommand command, CancellationToken ct = default)
    {
        try
        {
            logger.LogInformation("Команда {Type} от пользователя {UserId}", command.Type, command.TelegramUserId);
            switch (command.Type)
            {
                case BotCommandType.Start: await HandleStartAsync(command.TelegramUserId, ct); break;
                case BotCommandType.Add: await HandleAddAsync(command, ct); break;
                case BotCommandType.Remove: await HandleRemoveAsync(command, ct); break;
                case BotCommandType.List: await HandleListAsync(command.TelegramUserId, ct); break;
                case BotCommandType.News: await HandleNewsAsync(command, ct); break;
                case BotCommandType.Next: await HandleNextAsync(command.TelegramUserId, ct); break;
                case BotCommandType.Status: await HandleStatusAsync(command.TelegramUserId, ct); break;
                default: await ReplyAsync(command.TelegramUserId, "Неизвестная команда. Доступно: /start /add /remove /list /news /next /status", ct); break;
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ошибка обработки команды {Command}", command.Type);
            await ReplyAsync(command.TelegramUserId, "Произошла ошибка, попробуйте позже.", ct);
        }
    }

    private async Task HandleStartAsync(long telegramUserId, CancellationToken ct) =>
        await ReplyAsync(telegramUserId, """
            Привет! Я собираю новости из каналов и делаю дайджест.
            /add @username — добавить канал
            /remove @username — удалить канал
            /list — список каналов
            /news — собрать дайджест
            /news 24 — новости за последние 24 часа
            /next — следующая новость
            /status — статус очереди
            """, ct);

    private async Task HandleAddAsync(BotCommand command, CancellationToken ct)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var userRepo = scope.ServiceProvider.GetRequiredService<IUserRepository>();
        var channelRepo = scope.ServiceProvider.GetRequiredService<IUserChannelRepository>();
        var userbot = scope.ServiceProvider.GetRequiredService<IUserbotClient>();

        if (string.IsNullOrWhiteSpace(command.Argument))
        {
            await ReplyAsync(command.TelegramUserId, "Формат: /add @username или https://t.me/username", ct);
            return;
        }

        var channelId = await userbot.ResolveChannelAsync(command.Argument, ct);
        if (channelId is null or 0)
        {
            await ReplyAsync(command.TelegramUserId, "Не удалось определить канал. Укажите публичный канал.", ct);
            return;
        }

        var user = await userRepo.GetOrCreateAsync(command.TelegramUserId, ct);
        var existing = await channelRepo.GetByChannelIdAsync(user.Id, channelId.Value, ct);
        if (existing is not null)
        {
            await ReplyAsync(command.TelegramUserId, "Канал уже добавлен.", ct);
            return;
        }

        var username = NormalizeUsername(command.Argument);
        await channelRepo.AddAsync(new UserChannel { UserId = user.Id, ChannelId = channelId.Value, ChannelUsername = username, Title = username }, ct);
        await ReplyAsync(command.TelegramUserId, $"Канал @{username.TrimStart('@')} добавлен.", ct);
    }

    private async Task HandleRemoveAsync(BotCommand command, CancellationToken ct)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var userRepo = scope.ServiceProvider.GetRequiredService<IUserRepository>();
        var channelRepo = scope.ServiceProvider.GetRequiredService<IUserChannelRepository>();

        if (string.IsNullOrWhiteSpace(command.Argument))
        {
            await ReplyAsync(command.TelegramUserId, "Формат: /remove @username или id", ct);
            return;
        }

        var user = await userRepo.GetOrCreateAsync(command.TelegramUserId, ct);
        var argument = command.Argument.TrimStart('@');
        var channels = await channelRepo.GetAllForUserAsync(user.Id, ct);
        var match = channels.FirstOrDefault(c => c.ChannelUsername.Equals(argument, StringComparison.OrdinalIgnoreCase));

        if (match is null)
        {
            await ReplyAsync(command.TelegramUserId, "Канал не найден.", ct);
            return;
        }

        await channelRepo.DeleteAsync(match.Id, ct);
        await ReplyAsync(command.TelegramUserId, $"Канал @{argument} удалён.", ct);
    }

    private async Task HandleListAsync(long telegramUserId, CancellationToken ct)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var userRepo = scope.ServiceProvider.GetRequiredService<IUserRepository>();
        var channelRepo = scope.ServiceProvider.GetRequiredService<IUserChannelRepository>();

        var user = await userRepo.GetOrCreateAsync(telegramUserId, ct);
        var channels = await channelRepo.GetAllForUserAsync(user.Id, ct);

        if (channels.Count == 0)
        {
            await ReplyAsync(telegramUserId, "Каналов пока нет. Добавьте через /add @username.", ct);
            return;
        }

        var text = string.Join("\n", channels.Select(c => $"@{c.ChannelUsername.TrimStart('@')}"));
        await ReplyAsync(telegramUserId, $"Ваши каналы:\n{text}", ct);
    }

    private async Task HandleNewsAsync(BotCommand command, CancellationToken ct)
    {
        var telegramUserId = command.TelegramUserId;
        TimeSpan? lookbackPeriod = null;
        if (!string.IsNullOrWhiteSpace(command.Argument))
        {
            if (!int.TryParse(command.Argument, NumberStyles.None, CultureInfo.InvariantCulture, out var hours) || hours is < 1 or > 720)
            {
                await ReplyAsync(telegramUserId, "Формат: /news или /news <часы> (от 1 до 720). Например: /news 24.", ct);
                return;
            }

            lookbackPeriod = TimeSpan.FromHours(hours);
        }

        await using var scope = scopeFactory.CreateAsyncScope();
        var userRepo = scope.ServiceProvider.GetRequiredService<IUserRepository>();
        var collector = scope.ServiceProvider.GetRequiredService<INewsCollector>();
        var digestService = scope.ServiceProvider.GetRequiredService<IDigestService>();
        var queueRepo = scope.ServiceProvider.GetRequiredService<IDigestQueueRepository>();
        var channelStateRepo = scope.ServiceProvider.GetRequiredService<IChannelStateRepository>();

        var user = await userRepo.GetOrCreateAsync(telegramUserId, ct);
        var result = await collector.CollectNewsAsync(user.Id, ct, lookbackPeriod);

        if (result.Messages.Count == 0)
        {
            var periodMessage = lookbackPeriod is null ? string.Empty : $" за последние {(int)lookbackPeriod.Value.TotalHours} ч.";
            await ReplyAsync(telegramUserId, $"Новых новостей{periodMessage} нет.", ct);
            return;
        }

        var items = await digestService.CreateDigestAsync(user.Id, result.Messages, ct);
        var queue = new DigestQueue { UserId = user.Id, Status = DigestQueueStatus.Active, SourceBatchPath = result.BatchPath };
        await queueRepo.CreateQueueAsync(queue, ct);
        foreach (var item in items) item.DigestQueueId = queue.Id;
        await queueRepo.AddItemsAsync(items, ct);

        if (lookbackPeriod is null)
        {
            foreach (var channelId in result.ProcessedChannelIds)
                await channelStateRepo.UpdateLastDigestAsync(user.Id, channelId, DateTime.UtcNow, 0, ct);
        }

        var message = items.Count == 0
            ? "Дайджест пуст после обработки LLM."
            : $"Дайджест готов: {items.Count} новостей. Отправь /next.";
        await ReplyAsync(telegramUserId, message, ct);
    }

    private async Task HandleNextAsync(long telegramUserId, CancellationToken ct)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var userRepo = scope.ServiceProvider.GetRequiredService<IUserRepository>();
        var queueService = scope.ServiceProvider.GetRequiredService<IDigestQueueService>();

        var user = await userRepo.GetOrCreateAsync(telegramUserId, ct);
        var item = await queueService.GetNextItemAsync(user.Id, ct);

        if (item is null)
        {
            await ReplyAsync(telegramUserId, "Дайджест закончился. /news — новый сбор.", ct);
            return;
        }

        try
        {
            await newsLinkSender.SendNewsLinkAsync(telegramUserId, item, ct);
            await queueService.MarkAsSentAsync(item.Id, ct);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("не начал диалог"))
        {
            // Этот catch не должен срабатывать, т.к. мы в контексте активного чата с пользователем
            // Но на всякий случай обрабатываем
            logger.LogError(ex, "Неожиданная ошибка: пользователь {UserId} не начал диалог, хотя команда пришла", telegramUserId);
            await ReplyAsync(telegramUserId, 
                "⚠️ Внутренняя ошибка: бот не может отправить сообщение. Попробуйте заново запустить бота через /start.", ct);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("заблокировал"))
        {
            logger.LogWarning("Пользователь {UserId} заблокировал бота", telegramUserId);
            await ReplyAsync(telegramUserId, 
                "⚠️ Не удалось отправить новость — вы заблокировали бота. Разблокируйте и попробуйте снова.", ct);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Не удалось отправить новость {ItemId} пользователю {UserId}", item.Id, telegramUserId);
            await ReplyAsync(telegramUserId, 
                $"⚠️ Не удалось отправить новость.\n\nПрямая ссылка: {item.MessageUrl}", ct);
        }
    }

    private async Task HandleStatusAsync(long telegramUserId, CancellationToken ct)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var userRepo = scope.ServiceProvider.GetRequiredService<IUserRepository>();
        var queueRepo = scope.ServiceProvider.GetRequiredService<IDigestQueueRepository>();

        var user = await userRepo.GetOrCreateAsync(telegramUserId, ct);
        var queue = await queueRepo.GetCurrentQueueAsync(user.Id, ct);

        if (queue is null)
        {
            await ReplyAsync(telegramUserId, "Активных очередей нет. /news — собрать дайджест.", ct);
            return;
        }

        var unsent = await queueRepo.CountUnsentAsync(user.Id, ct);
        await ReplyAsync(telegramUserId, $"Очередь активна, осталось новостей: {unsent}.", ct);
    }

    private async Task ReplyAsync(long telegramUserId, string text, CancellationToken ct) =>
        await botClient.SendMessage(new Telegram.Bot.Types.ChatId(telegramUserId), text, cancellationToken: ct);

    private static string NormalizeUsername(string link)
    {
        if (link.Contains("t.me/", StringComparison.Ordinal))
            link = link[(link.LastIndexOf("t.me/", StringComparison.Ordinal) + 5)..];
        return link.Trim('/', ' ', '@');
    }
}