using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using IceBackend.Domain.Services;
using Microsoft.Extensions.Logging;

namespace IceBackend.Infrastructure.Services
{
    public class MojangSessionValidator : IMojangSessionValidator
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<MojangSessionValidator> _logger;

        public MojangSessionValidator(HttpClient httpClient, ILogger<MojangSessionValidator> logger)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<bool> ValidateSessionAsync(string username, string accessToken)
        {
            if (string.IsNullOrWhiteSpace(username)) return false;
            if (string.IsNullOrWhiteSpace(accessToken)) return false;

            try
            {
                // Endpoint oficial de Microsoft/Mojang para verificar el perfil del jugador autenticado
                using var request = new HttpRequestMessage(HttpMethod.Get, "https://api.minecraftservices.com/minecraft/profile");
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                var response = await _httpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Mojang Session validation failed with status: {StatusCode}", response.StatusCode);
                    return false;
                }

                var content = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(content);
                
                if (doc.RootElement.TryGetProperty("name", out var nameProp))
                {
                    var profileName = nameProp.GetString();
                    
                    // Validamos que el nombre en el perfil premium coincida con el nombre de usuario provisto
                    bool isMatch = string.Equals(profileName, username, StringComparison.OrdinalIgnoreCase);
                    if (!isMatch)
                    {
                        _logger.LogWarning("Mojang Profile name '{ProfileName}' does not match expected username '{Username}'", profileName, username);
                    }
                    return isMatch;
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al consultar la API de validación de Mojang para el jugador: {Username}", username);
                return false;
            }
        }
    }
}
