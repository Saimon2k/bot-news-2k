namespace NewsDigest.Core.Models;

public enum BotCommandType { Start, Add, Remove, List, News, Next, Status, Unknown }

public sealed record BotCommand(BotCommandType Type, string Argument, long TelegramUserId);

public static class BotCommandParser
{
    public static BotCommand Parse(string? text, long telegramUserId)
    {
        if (string.IsNullOrWhiteSpace(text)) return new BotCommand(BotCommandType.Unknown, string.Empty, telegramUserId);

        var parts = text.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
        var command = parts[0].ToLowerInvariant();
        var argument = parts.Length > 1 ? parts[1].Trim() : string.Empty;

        return command switch
        {
            "/start" => new BotCommand(BotCommandType.Start, string.Empty, telegramUserId),
            "/add" => new BotCommand(BotCommandType.Add, argument, telegramUserId),
            "/remove" => new BotCommand(BotCommandType.Remove, argument, telegramUserId),
            "/list" => new BotCommand(BotCommandType.List, string.Empty, telegramUserId),
            "/news" => new BotCommand(BotCommandType.News, argument, telegramUserId),
            "/next" => new BotCommand(BotCommandType.Next, string.Empty, telegramUserId),
            "/status" => new BotCommand(BotCommandType.Status, string.Empty, telegramUserId),
            _ => new BotCommand(BotCommandType.Unknown, text, telegramUserId),
        };
    }
}