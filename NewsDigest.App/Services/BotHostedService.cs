using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NewsDigest.Core.Interfaces;
using NewsDigest.Core.Models;
using NewsDigest.Core.Options;
using NewsDigest.Infrastructure.Data;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace NewsDigest.App.Services;

public class BotHostedService(
    ILogger<BotHostedService> logger,
    IHostApplicationLifetime lifetime,
    IBotService botService,
    IUserbotAuthService authService,
    ITelegramBotClient botClient,
    IServiceScopeFactory scopeFactory,
    IOptions<StorageOptions> storageOptions) : IHostedService
{
    private CancellationTokenSource? _cts;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Запуск NewsDigest...");

        var storage = storageOptions.Value;
        Directory.CreateDirectory(storage.BatchDirectory);
        var dbDirectory = Path.GetDirectoryName(storage.DatabasePath);
        if (!string.IsNullOrEmpty(dbDirectory)) Directory.CreateDirectory(dbDirectory);

        await using (var scope = scopeFactory.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await db.Database.EnsureCreatedAsync(cancellationToken);
        }

        var authorized = await authService.LoginAsync(cancellationToken);
        if (!authorized)
        {
            logger.LogError("Не удалось авторизовать юзербота, остановка хоста");
            lifetime.StopApplication();
            return;
        }

        _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        _ = Task.Run(() => PollAsync(_cts.Token));
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _cts?.Cancel();
        logger.LogInformation("Остановка NewsDigest");
        await Task.CompletedTask;
    }

    private async Task PollAsync(CancellationToken ct)
    {
        var offset = 0;
        try
        {
            while (!ct.IsCancellationRequested)
            {
                var updates = await botClient.GetUpdates(offset, limit: 100, timeout: 30, cancellationToken: ct);
                foreach (var update in updates)
                {
                    if (update.Message?.Text is null) continue;

                    var command = BotCommandParser.Parse(update.Message.Text, update.Message.Chat.Id);
                    if (command.Type == BotCommandType.Unknown) continue;

                    logger.LogInformation("Обработка команды {Text} от {UserId}", update.Message.Text, command.TelegramUserId);
                    await botService.HandleCommandAsync(command, ct);
                    offset = update.Id + 1;
                }
            }
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            logger.LogInformation("Поллинг остановлен");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ошибка в цикле поллинга");
        }
    }
}