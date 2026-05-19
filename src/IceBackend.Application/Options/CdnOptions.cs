namespace IceBackend.Application.Options
{
    public class CdnOptions
    {
        public string BaseUrl { get; set; } = string.Empty;
        public int UrlExpirationMinutes { get; set; } = 15;
        public string SigningSecret { get; set; } = string.Empty;
    }
}
