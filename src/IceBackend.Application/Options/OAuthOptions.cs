namespace IceBackend.Application.Options
{
    public class OAuthOptions
    {
        public const string SectionName = "OAuth";

        public MicrosoftOAuthConfig Microsoft { get; set; } = null!;
        public GoogleOAuthConfig Google { get; set; } = null!;
    }

    public class MicrosoftOAuthConfig
    {
        public string ClientId { get; set; } = null!;
        public string ClientSecret { get; set; } = null!;
        public string TenantId { get; set; } = "consumers"; // consumers = MSA (cuentas personales)
        public string RedirectUri { get; set; } = null!;
    }

    public class GoogleOAuthConfig
    {
        public string ClientId { get; set; } = null!;
        public string ClientSecret { get; set; } = null!;
        public string RedirectUri { get; set; } = null!;
    }
}
