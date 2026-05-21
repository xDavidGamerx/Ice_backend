using System.ComponentModel.DataAnnotations;

namespace IceBackend.Application.Options
{
    public class OAuthProviderOptions
    {
        [Required]
        public string ClientId { get; init; } = string.Empty;

        [Required]
        public string ClientSecret { get; init; } = string.Empty;

        public string? TenantId { get; init; }

        [Required]
        [Url(ErrorMessage = "RedirectUri must be a valid URL/URI")]
        public string RedirectUri { get; init; } = string.Empty;
    }

    public class OAuthOptions
    {
        public const string SectionName = "OAuth";

        [Required]
        public OAuthProviderOptions Microsoft { get; init; } = new();

        [Required]
        public OAuthProviderOptions Google { get; init; } = new();
    }
}
