using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using NewsDigest.App.Services;
using NewsDigest.Core.Interfaces;
using NewsDigest.Core.Options;
using NewsDigest.Core.Services;
using NewsDigest.Infrastructure.Data;
using NewsDigest.Infrastructure.Repositories;
using NewsDigest.Infrastructure.Wrappers;
using Telegram.Bot;

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
        try
        {
            var host = Host.CreateDefaultBuilder(args)
                .UseContentRoot(GetContentRoot())
                .ConfigureAppConfiguration((context, builder) =>
                {
                    // Загружаем секреты из корня проекта
                    var rootPath = Directory.GetParent(GetContentRoot())?.FullName ?? GetContentRoot();
                    var secretsJsonPath = Path.Combine(rootPath, ".secrets.json");
                    if (File.Exists(secretsJsonPath))
                        builder.AddJsonFile(secretsJsonPath, optional: true, reloadOnChange: false);
                    
                    builder.AddUserSecrets(typeof(Program).Assembly, optional: true);
                })
                // .UseSerilog((context, services, configuration) => configuration
                //     .Enrich.FromLogContext()
                //     .WriteTo.Console()
                //     .WriteTo.File(Path.Combine("logs", "digest-.log"), rollingInterval: RollingInterval.Day))
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
                        var telegramOptions = sp.GetService<IOptions<TelegramOptions>>().Value;
                        return new TelegramBotClient(telegramOptions.BotToken);
                    });
                    services.AddSingleton<INewsLinkSender, TelegramNewsLinkSender>();
                    services.AddSingleton<IBotService, TelegramBotService>();

                    services.AddHostedService<BotHostedService>();
                })
                .Build();

            await host.RunAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"КРИТИЧЕСКАЯ ОШИБКА: {ex.Message}");
            Console.WriteLine($"StackTrace: {ex.StackTrace}");
            // Log.Fatal(ex, "Приложение завершилось с ошибкой");
        }
        finally
        {
            // Log.CloseAndFlush();
        }
    }
}