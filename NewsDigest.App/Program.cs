namespace NewsDigest.App;

internal static class Program
{
    private static string GetContentRoot()
    {
        var currentDir = Directory.GetCurrentDirectory();
        // Если уже в папке NewsDigest.App, используем текущую
        if (currentDir.EndsWith("NewsDigest.App", StringComparison.OrdinalIgnoreCase))
            return currentDir;

        // Иначе добавляем NewsDigest.App к пути
        var projectDir = Path.Combine(currentDir, "NewsDigest.App");
        return Directory.Exists(projectDir) ? projectDir : currentDir;
    }

    public static async Task Main(string[] args)
    {
        using var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        var logger = loggerFactory.CreateLogger(typeof(Program).FullName ?? "Program");
        IHost? host = null;

        try
        {
            host = Host.CreateDefaultBuilder(args)
                .UseContentRoot(GetContentRoot())
                .ConfigureAppConfiguration((context, builder) =>
                    builder.AddUserSecrets(typeof(Program).Assembly, optional: true))
                .ConfigureServices((context, services) =>
                {
                    var config = context.Configuration;

                    services.Configure<TelegramOptions>(config.GetSection("Telegram"));
                    services.Configure<OllamaOptions>(config.GetSection("Ollama"));
                    services.Configure<StorageOptions>(config.GetSection("Storage"));

                    services.AddOptions<TelegramOptions>().ValidateDataAnnotations();
                    services.AddOptions<OllamaOptions>().ValidateDataAnnotations();
                    services.AddOptions<StorageOptions>().ValidateDataAnnotations();

                    var app = config.GetSection("Storage");
                    var dbPath = string.IsNullOrEmpty(app["DatabasePath"]) ? "news-digest.db" : app["DatabasePath"];
                    services.AddDbContext<AppDbContext>(o => o.UseSqlite($"Data Source={dbPath}"));

                    services.AddScoped<IUserRepository, UserRepository>();
                    services.AddScoped<IUserChannelRepository, UserChannelRepository>();
                    services.AddScoped<IChannelStateRepository, ChannelStateRepository>();
                    services.AddScoped<IDigestQueueRepository, DigestQueueRepository>();
                    services.AddScoped<INewsCollector, NewsCollector>();
                    services.AddScoped<IDigestService, DigestService>();
                    services.AddScoped<IDigestQueueService, DigestQueueService>();

                    services.AddSingleton<IOllamaClient, OllamaClient>();
                    services.AddSingleton<IUserbotClient, UserbotClient>();
                    services.AddSingleton<IUserbotAuthService, UserbotAuthService>();

                    services.AddSingleton<ITelegramBotClient>(sp =>
                    {
                        var telegramOptions = sp.GetRequiredService<IOptions<TelegramOptions>>().Value;
                        return new TelegramBotClient(telegramOptions.BotToken);
                    });
                    services.AddSingleton<INewsLinkSender, TelegramNewsLinkSender>();
                    services.AddSingleton<IBotService, TelegramBotService>();

                    services.AddHostedService<BotHostedService>();
                })
                .Build();

            logger.LogInformation("Хост запущен, старт приложения");

            await host.RunAsync();
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "Приложение завершилось с ошибкой");
        }
        finally
        {
            host?.Dispose();
        }
    }
}