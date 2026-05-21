using System.ComponentModel.DataAnnotations;

namespace IceBackend.Application.Options
{
    public class AuthOptions
    {
        public const string SectionName = "Auth";

        [Required]
        [Range(1, 720)]
        public int SessionTtlHours { get; init; }
    }
}
