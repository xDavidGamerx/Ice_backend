using IceBackend.Application.Interfaces;
using IceBackend.Domain.Enums;

namespace IceBackend.Api.Middleware
{
    /// <summary>
    /// Implementación mutable del contexto de cliente.
    /// Registrada como Scoped para que el middleware la pueble y los Use Cases la lean.
    /// </summary>
    public class ClientContext : IClientContext
    {
        public AssetArchitecture? Architecture { get; set; }
    }

    /// <summary>
    /// Middleware que extrae el header X-Client-Architecture y lo inyecta en IClientContext.
    /// Si el header no está presente o es inválido, Architecture queda null.
    /// </summary>
    public class ClientContextMiddleware
    {
        private readonly RequestDelegate _next;

        public ClientContextMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, ClientContext clientContext)
        {
            if (context.Request.Headers.TryGetValue("X-Client-Architecture", out var archHeader))
            {
                if (Enum.TryParse<AssetArchitecture>(archHeader.ToString(), ignoreCase: true, out var architecture))
                {
                    clientContext.Architecture = architecture;
                }
            }

            await _next(context);
        }
    }
}
