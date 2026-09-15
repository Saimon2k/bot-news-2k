using System.ComponentModel.DataAnnotations;

namespace NewsDigest.Core.Options
{
    public class OllamaOptions
    {
        [Required] public string Url { get; set; } = string.Empty;
        [Required] public string Model { get; set; } = string.Empty;
    }
}