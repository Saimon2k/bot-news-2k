using System.ComponentModel.DataAnnotations;

namespace NewsDigest.Core.Options
{
    public class StorageOptions
    {
        [Required] public string DatabasePath { get; set; } = string.Empty;
        [Required] public string BatchDirectory { get; set; } = string.Empty;
    }
}