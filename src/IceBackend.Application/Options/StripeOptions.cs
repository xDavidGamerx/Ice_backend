using System.ComponentModel.DataAnnotations;

namespace IceBackend.Application.Options
{
    public class StripeOptions
    {
        public const string SectionName = "Stripe";

        [Required(ErrorMessage = "Stripe SecretKey is missing")]
        [MinLength(10)]
        public string SecretKey { get; init; } = string.Empty;

        [Required(ErrorMessage = "Stripe WebhookSecret is missing")]
        [MinLength(10)]
        public string WebhookSecret { get; init; } = string.Empty;
    }
}
