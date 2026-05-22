using System.ComponentModel.DataAnnotations;

namespace IceBackend.Application.Options
{
    public class AuthOptions
    {
        public const string SectionName = "Auth";

        [Required]
        [Range(1, 720)]
        public int SessionTtlHours { get; init; }

        [Required]
        [Range(4, 15)]
        public int BcryptWorkFactor { get; init; } = 10;

        [Required]
        public string LegacySalt { get; init; } = "IceLauncherSecretSalt";
    }
}
