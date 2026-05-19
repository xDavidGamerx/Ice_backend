namespace IceBackend.Application.Options
{
    public class StripeOptions
    {
        public const string SectionName = "Stripe";

        /// <summary>Clave secreta de la API de Stripe (sk_live_... / sk_test_...).</summary>
        public string SecretKey { get; set; } = null!;

        /// <summary>Webhook Signing Secret provisto por el dashboard de Stripe (whsec_...).</summary>
        public string WebhookSecret { get; set; } = null!;
    }
}
