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

/// <summary>
/// Фоновый сервис для управления жизненным циклом Telegram-бота и обработки входящих обновлений.
/// </summary>
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
    private Task? _pollingTask;
    private Task? _authTask;

    /// <summary>
    /// Запуск сервиса: инициализация хранилища, БД, авторизация и начало опроса обновлений.
    /// </summary>
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Запуск NewsDigest...");

        try
        {
            // Инициализация файловой системы
            await InitializeStorageAsync(cancellationToken);

            // Инициализация базы данных
            await InitializeDatabaseAsync(cancellationToken);

            // Проверяем Bot API и снимаем webhook: одновременно использовать webhook и polling нельзя.
            var bot = await botClient.GetMe(cancellationToken);
            logger.LogInformation("Подключен Telegram-бот @{Username}", bot.Username);

            var webhookInfo = await botClient.GetWebhookInfo(cancellationToken);
            if (!string.IsNullOrEmpty(webhookInfo.Url))
            {
                logger.LogWarning("Удаляю webhook {WebhookUrl}: приложение использует long polling", webhookInfo.Url);
                await botClient.DeleteWebhook(cancellationToken: cancellationToken);
            }

            // Приём сообщений не должен ждать интерактивной авторизации юзербота.
            _cts = CancellationTokenSource.CreateLinkedTokenSource(lifetime.ApplicationStopping);
            _pollingTask = PollAsync(_cts.Token, bot.Username);
            _authTask = AuthenticateUserbotAsync(_cts.Token);

            logger.LogInformation("Опрос сообщений запущен; авторизация юзербота выполняется отдельно");
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "Критическая ошибка при запуске NewsDigest");
            lifetime.StopApplication();
            throw;
        }
    }

    /// <summary>
    /// Остановка сервиса с graceful shutdown опроса обновлений.
    /// </summary>
    public async Task StopAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Остановка NewsDigest...");

        try
        {
            // Отмена токена для завершения опроса
            _cts?.Cancel();

            // Ожидание фоновых задач с таймаутом.
            var tasks = new[] { _pollingTask, _authTask }.OfType<Task>().ToArray();
            if (tasks.Length > 0)
            {
                var allTasks = Task.WhenAll(tasks);
                var completedTask = await Task.WhenAny(allTasks, Task.Delay(TimeSpan.FromSeconds(5), cancellationToken));
                if (completedTask != allTasks)
                {
                    logger.LogWarning("Фоновые задачи не завершились в течение таймаута");
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ошибка при остановке NewsDigest");
        }
        finally
        {
            _cts?.Dispose();
            logger.LogInformation("NewsDigest остановлен");
        }
    }

    private async Task AuthenticateUserbotAsync(CancellationToken ct)
    {
        try
        {
            if (!await authService.LoginAsync(ct))
                logger.LogError("Не удалось авторизовать юзербота. Получение сообщений ботом продолжится, но функции чтения каналов будут недоступны");
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            // Graceful shutdown.
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ошибка авторизации юзербота; получение сообщений ботом продолжится");
        }
    }

    /// <summary>
    /// Инициализация директорий хранилища.
    /// </summary>
    private async Task InitializeStorageAsync(CancellationToken cancellationToken)
    {
        var storage = storageOptions.Value;

        try
        {
            Directory.CreateDirectory(storage.BatchDirectory);
            logger.LogDebug("Директория батчей создана: {Path}", storage.BatchDirectory);

            var dbDirectory = Path.GetDirectoryName(storage.DatabasePath);
            if (!string.IsNullOrEmpty(dbDirectory))
            {
                Directory.CreateDirectory(dbDirectory);
                logger.LogDebug("Директория БД создана: {Path}", dbDirectory);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ошибка при создании директорий хранилища");
            throw;
        }

        await Task.CompletedTask; // Для консистентности async-методов
    }

    /// <summary>
    /// Инициализация базы данных.
    /// </summary>
    private async Task InitializeDatabaseAsync(CancellationToken cancellationToken)
    {
        try
        {
            await using var scope = scopeFactory.CreateAsyncScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await db.Database.EnsureCreatedAsync(cancellationToken);
            logger.LogDebug("База данных инициализирована");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ошибка при инициализации базы данных");
            throw;
        }
    }

    /// <summary>
    /// Основной цикл опроса обновлений от Telegram Bot API.
    /// </summary>
    private async Task PollAsync(CancellationToken ct, string? botUsername)
    {
        var offset = 0;
        const int UpdateLimit = 100;
        const int TimeoutSeconds = 30;

        logger.LogInformation("Начат опрос обновлений с offset={Offset}", offset);

        try
        {
            while (!ct.IsCancellationRequested)
            {
                try
                {
                    var updates = await botClient.GetUpdates(
                        offset: offset,
                        limit: UpdateLimit,
                        timeout: TimeoutSeconds,
                        cancellationToken: ct);

                    foreach (var update in updates)
                    {
                        // Всегда обновляем offset, даже если апдейт игнорируется
                        offset = update.Id + 1;

                        // Игнорируем апдейты без текстового сообщения
                        if (update.Message?.Text is null)
                        {
                            logger.LogDebug("Пропущен апдейт {UpdateId} без текстового сообщения", update.Id);
                            continue;
                        }

                        // Парсинг и обработка команды
                        var command = BotCommandParser.Parse(update.Message.Text, update.Message.Chat.Id, botUsername);
                        if (command.Type == BotCommandType.Unknown)
                        {
                            logger.LogDebug("Неизвестная команда: {Text} от {UserId}", 
                                update.Message.Text, update.Message.Chat.Id);
                            continue;
                        }

                        logger.LogInformation("Обработка команды {Text} от {UserId}", 
                            update.Message.Text, command.TelegramUserId);

                        try
                        {
                            await botService.HandleCommandAsync(command, ct);
                        }
                        catch (Exception cmdEx)
                        {
                            logger.LogError(cmdEx, "Ошибка при обработке команды {CommandType} от {UserId}", 
                                command.Type, command.TelegramUserId);
                            // Продолжаем обработку других апдейтов
                        }
                    }

                    // Небольшая задержка между итерациями при отсутствии апдейтов
                    if (updates.Length == 0)
                    {
                        await Task.Delay(100, ct);
                    }
                }
                catch (OperationCanceledException) when (ct.IsCancellationRequested)
                {
                    // Graceful shutdown - не логируем как ошибку
                    throw;
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Ошибка при получении обновлений, повтор через 5 секунд");
                    await Task.Delay(TimeSpan.FromSeconds(5), ct);
                }
            }
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            logger.LogInformation("Опрос обновлений остановлен (offset={Offset})", offset);
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "Критическая ошибка в цикле опроса");
            lifetime.StopApplication();
        }
    }
}
