using System.ComponentModel.DataAnnotations;

namespace NewsDigest.Core.Options
{
    public class TelegramOptions
    {
        [Required] public string ApiId { get; set; } = string.Empty;
        [Required] public string ApiHash { get; set; } = string.Empty;
        [Required] public string PhoneNumber { get; set; } = string.Empty;
        [Required] public string SessionPath { get; set; } = string.Empty;
        [Required] public string BotToken { get; set; } = string.Empty;
        public string MTProxyUrl { get; set; } = string.Empty;
    }
}