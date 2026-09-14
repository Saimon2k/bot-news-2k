using Microsoft.Extensions.Logging;
using Serilog;

namespace NewsDigest.App;

// File-scoped namespace for the program entry point
internal static class Program
{
    public static async Task Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .WriteTo.File("logs/app-.txt", rollingInterval: RollingInterval.Day)
            .CreateLogger();

        var host = Host.CreateDefaultBuilder(args)
            .ConfigureServices((context, services) =>
            {
                // Dependency Injection registration logic will go here
            })
            .UseSerilog(Log.Logger)
            .Build();

        await host.RunAsync();
    }
}
    }
}