using TL;
using WTelegram;

namespace NewsDigest.Infrastructure.Wrappers;

public class UserbotClient(ILogger<UserbotClient> logger, IOptions<TelegramOptions> options) : IUserbotClient
{
    private readonly TelegramOptions _options = options.Value;
    private Client? _client;

    private Client Client => _client ?? throw new InvalidOperationException("Юзербот не инициализирован");

    public async Task<bool> ConnectAsync(CancellationToken ct = default)
    {
        if (_client is not null) return true;

        _client = new Client(what => what switch
        {
            "api_id" => _options.ApiId,
            "api_hash" => _options.ApiHash,
            "phone_number" => _options.PhoneNumber,
            //"session_pathname" => _options.SessionPath,
            _ => null!,
        });
        _client.FloodRetryThreshold = 2;
        _client.MTProxyUrl = _options.MTProxyUrl;

        Helpers.Log = (level, message) => logger.Log((LogLevel)Math.Clamp(level, 0, 6), message);

        try
        {
            var loginInfo = _options.PhoneNumber;
            while (_client.User is null)
            {
                var what = await _client.Login(loginInfo);
                switch (what)
                {
                    case "verification_code":
                        loginInfo = Prompt("Введите код из Telegram: ");
                        break;
                    case "password":
                        loginInfo = Prompt("Введите пароль 2FA: ");
                        break;
                    case "name":
                        loginInfo = Prompt("Введите ваше имя: ");
                        break;
                    case "email":
                    case "email_verification_code":
                        loginInfo = Prompt("Введите email: ");
                        break;
                    case null:
                        ct.ThrowIfCancellationRequested();
                        break;
                    default:
                        logger.LogWarning("Неизвестный шаг авторизации: {What}", what);
                        loginInfo = Prompt("Введите запрошенное значение: ");
                        break;
                }
            }

            logger.LogInformation("Юзербот авторизован как {Id}", _client.User!.id);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ошибка подключения/авторизации юзербота");
            return false;
        }
    }

    public async Task<IReadOnlyList<ChannelMessage>> GetHistoryAsync(long channelId, string? channelUsername, DateTime from, DateTime to, CancellationToken ct = default)
    {
        var peer = await ResolvePeerAsync(channelId, channelUsername, ct);
        const int pageSize = 100;
        var messages = new List<TL.Message>();
        var offsetId = 0;

        while (true)
        {
            ct.ThrowIfCancellationRequested();
            var result = await Client.Messages_GetHistory(peer,
                offset_id: offsetId, offset_date: offsetId == 0 ? to : default, add_offset: 0, limit: pageSize, max_id: 0, min_id: 0, hash: 0);
            var page = result.Messages.OfType<TL.Message>().ToList();
            if (page.Count == 0) break;

            messages.AddRange(page.Where(m => m.date >= from && m.date <= to));

            var oldestMessage = page[^1];
            if (oldestMessage.date < from || result.Messages.Length < pageSize) break;

            offsetId = oldestMessage.id;
        }

        return messages
            .Select(m => new ChannelMessage(channelId, channelUsername, m.id, m.date, m.message ?? string.Empty, m.media is not null))
            .ToList();
    }

    public Task<string> GetMessageUrlAsync(long channelId, string? channelUsername, long messageId, CancellationToken ct = default) =>
        Task.FromResult(string.IsNullOrEmpty(channelUsername)
            ? $"https://t.me/c/{channelId}/{messageId}"
            : $"https://t.me/{channelUsername.TrimStart('@')}/{messageId}");

    public async Task<long?> ResolveChannelAsync(string link, CancellationToken ct = default)
    {
        var username = NormalizeUsername(link);
        if (string.IsNullOrEmpty(username)) return null;

        try
        {
            var resolved = await Client.Contacts_ResolveUsername(username, null);
            return resolved.Channel?.id;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Не удалось разрешить канал {Link}", link);
            return null;
        }
    }

    private async Task<InputPeer> ResolvePeerAsync(long channelId, string? channelUsername, CancellationToken ct)
    {
        if (!string.IsNullOrEmpty(channelUsername))
        {
            var resolved = await Client.Contacts_ResolveUsername(channelUsername.TrimStart('@'), null);
            var channel = resolved.Channel;
            if (channel is not null) return new InputPeerChannel(channel.id, channel.access_hash);
        }

        logger.LogWarning("Нет username или запрещённый канал {ChannelId}, использую InputPeerChannel без access_hash", channelId);
        return new InputPeerChannel(channelId, 0);
    }

    private static string NormalizeUsername(string link)
    {
        var text = link.Trim().TrimStart('@');
        if (text.Contains("t.me/", StringComparison.Ordinal))
            text = text[(text.LastIndexOf("t.me/", StringComparison.Ordinal) + 5)..].Trim('/');
        return text.Split('/', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? text;
    }

    private static string Prompt(string message)
    {
        Console.Write(message);
        return Console.ReadLine() ?? string.Empty;
    }
}