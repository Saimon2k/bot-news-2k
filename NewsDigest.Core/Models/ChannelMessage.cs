namespace NewsDigest.Core.Models;

public sealed record ChannelMessage(long ChannelId, string? ChannelUsername, long MessageId, DateTime Date, string Text, bool HasMedia)
{
    public MessageType Classify() => (HasMedia, string.IsNullOrWhiteSpace(Text)) switch
    {
        (true, false) => MessageType.MediaWithCaption,
        (true, true) => MessageType.MediaWithoutText,
        _ => MessageType.Text,
    };
}