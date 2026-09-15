namespace NewsDigest.Core.Interfaces;

public interface IBotService
{
    Task HandleCommandAsync(BotCommand command, CancellationToken ct = default);
}