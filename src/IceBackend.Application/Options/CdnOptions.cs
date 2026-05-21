using System.ComponentModel.DataAnnotations;

namespace IceBackend.Application.Options
{
    public class CdnOptions
    {
        public const string SectionName = "Cdn";

        [Required(ErrorMessage = "CDN BaseUrl is missing")]
        [Url]
        public string BaseUrl { get; init; } = string.Empty;

        [Required]
        [Range(1, 1440)]
        public int UrlExpirationMinutes { get; init; }

        [Required(ErrorMessage = "CDN SigningSecret is missing")]
        [MinLength(10)]
        public string SigningSecret { get; init; } = string.Empty;
    }
}
