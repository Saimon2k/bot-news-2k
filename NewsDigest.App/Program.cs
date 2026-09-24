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
// using Serilog;
// using Serilog.Events;
using Telegram.Bot;

namespace NewsDigest.App;

internal static class Program
{
    public static async Task Main(string[] args)
    {
        // Log.Logger = new LoggerConfiguration()
        //     .MinimumLevel.Information()
        //     .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
        //     .Enrich.FromLogContext()
        //     .WriteTo.Console()
        //     .WriteTo.File(Path.Combine("logs", "digest-.log"), rollingInterval: RollingInterval.Day)
        //     .CreateLogger();

        try
        {
            var host = Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration(builder => builder.AddUserSecrets(typeof(Program).Assembly, optional: true))
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

                    services.AddOptions<TelegramOptions>().ValidateDataAnnotations().ValidateOnStart();
                    services.AddOptions<OllamaOptions>().ValidateDataAnnotations().ValidateOnStart();
                    services.AddOptions<StorageOptions>().ValidateDataAnnotations().ValidateOnStart();

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

            await host.RunAsync();
        }
        catch (Exception ex)
        {
            // Log.Fatal(ex, "Приложение завершилось с ошибкой");
        }
        finally
        {
            // Log.CloseAndFlush();
        }
    }
}