using IceBackend.Domain.Enums;

namespace IceBackend.Application.Interfaces
{
    /// <summary>
    /// Contexto del cliente que realiza la petición.
    /// Poblado por un middleware a partir del header X-Client-Architecture.
    /// Permite a los Use Cases conocer la arquitectura del Launcher sin acoplarse a HTTP.
    /// </summary>
    public interface IClientContext
    {
        /// <summary>
        /// Arquitectura reportada por el Launcher (Legacy, Modern o Universal).
        /// Null si el header no fue proporcionado.
        /// </summary>
        AssetArchitecture? Architecture { get; }
    }
}
