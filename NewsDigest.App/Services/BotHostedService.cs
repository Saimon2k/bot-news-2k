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

            // Авторизация юзербота
            var authorized = await authService.LoginAsync(cancellationToken);
            if (!authorized)
            {
                logger.LogError("Не удалось авторизовать юзербота, остановка хоста");
                lifetime.StopApplication();
                return;
            }

            // Запуск опроса обновлений
            _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            _pollingTask = Task.Run(() => PollAsync(_cts.Token), _cts.Token);

            logger.LogInformation("NewsDigest успешно запущен");
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

            // Ожидание завершения задачи опроса с таймаутом
            if (_pollingTask != null)
            {
                var completedTask = await Task.WhenAny(_pollingTask, Task.Delay(TimeSpan.FromSeconds(5), cancellationToken));
                if (completedTask != _pollingTask)
                {
                    logger.LogWarning("Задача опроса не завершилась в течение таймаута");
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
    private async Task PollAsync(CancellationToken ct)
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
                        var command = BotCommandParser.Parse(update.Message.Text, update.Message.Chat.Id);
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
