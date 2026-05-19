namespace IceBackend.Application.Options
{
    /// <summary>
    /// Opciones configurables para el módulo de autenticación.
    /// Mapeadas desde la sección "Auth" en appsettings.json.
    /// </summary>
    public class AuthOptions
    {
        public const string SectionName = "Auth";

        /// <summary>Tiempo de vida de la sesión en horas. Default: 8.</summary>
        public int SessionTtlHours { get; set; } = 8;
    }
}
