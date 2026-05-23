using System.Threading.Tasks;

namespace IceBackend.Domain.Services
{
    /// <summary>
    /// Servicio de infraestructura que verifica el token de sesión de Mojang en el backend.
    /// </summary>
    public interface IMojangSessionValidator
    {
        Task<bool> ValidateSessionAsync(string username, string accessToken);
    }
}
