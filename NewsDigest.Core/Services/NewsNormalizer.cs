using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace NewsDigest.Core.Services;

public static partial class NewsNormalizer
{
    [GeneratedRegex(@"[^\S\u00A0]+")]
    private static partial Regex WhitespaceRegex();

    [GeneratedRegex(@"https?://\S+")]
    private static partial Regex UrlRegex();

    [GeneratedRegex(@"@\w+")]
    private static partial Regex MentionRegex();

    [GeneratedRegex(@"[\p{Cs}\p{P}\p{S}]")]
    private static partial Regex EmojiPunctRegex();

    public static string Normalize(string? text)
    {
        if (string.IsNullOrWhiteSpace(text)) return string.Empty;

        var lowered = text.ToLowerInvariant();
        var noUrls = UrlRegex().Replace(lowered, " ");
        var noMentions = MentionRegex().Replace(noUrls, " ");
        var collapsed = WhitespaceRegex().Replace(noMentions, " ");
        return EmojiPunctRegex().Replace(collapsed, string.Empty).Trim();
    }

    public static string ComputeHash(string text) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(text)));

    public static IReadOnlyList<CollectedMessage> SelectCanonicals(IReadOnlyList<CollectedMessage> messages)
    {
        var seen = new Dictionary<string, CollectedMessage>(StringComparer.Ordinal);

        foreach (var message in messages.OrderBy(m => m.Date).ThenBy(m => m.MessageId))
        {
            if (string.IsNullOrWhiteSpace(message.NormalizedText))
            {
                message.IsCanonical = false;
                message.DuplicateOfMessageId = null;
                continue;
            }

            if (seen.TryGetValue(message.ContentHash, out var canonical))
            {
                message.IsCanonical = false;
                message.DuplicateOfMessageId = canonical.Id;
            }
            else
            {
                message.IsCanonical = true;
                message.DuplicateOfMessageId = null;
                seen[message.ContentHash] = message;
            }
        }

        return messages;
    }
}