namespace NewsDigest.Core.Options
{
    public class TelegramOptions
    {
        public string ApiId { get; set; } = string.Empty;
        public string ApiHash { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string SessionPath { get; set; } = string.Empty;
        public string BotToken { get; set; } = string.Empty;
    }
}