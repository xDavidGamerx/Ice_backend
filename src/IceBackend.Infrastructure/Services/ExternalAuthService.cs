using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using IceBackend.Application.DTOs;
using IceBackend.Application.Interfaces;
using IceBackend.Application.Options;
using IceBackend.Domain.Entities;
using IceBackend.Domain.Enums;
using IceBackend.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace IceBackend.Infrastructure.Services
{
    /// <summary>
    /// Implementación del flujo OAuth2 PKCE para Microsoft y Google.
    /// Es el único punto del sistema que realiza llamadas HTTP externas a proveedores de identidad.
    /// </summary>
    public class ExternalAuthService : IExternalAuthService
    {
        // ── Constantes de namespace de claves Redis ──────────────────────────────
        private const string PkcePrefix = "pkce:state:";
        private const string ReplayPrefix = "pkce:used:";
        private static readonly TimeSpan PkceTtl = TimeSpan.FromMinutes(5);
        private static readonly TimeSpan ReplayTtl = TimeSpan.FromMinutes(10); // margen extra post-TTL

        private readonly ApplicationDbContext _dbContext;
        private readonly ISessionCache _sessionCache;
        private readonly OAuthOptions _oauthOptions;
        private readonly AuthOptions _authOptions;
        private readonly HttpClient _httpClient;
        private readonly ILogger<ExternalAuthService> _logger;

        public ExternalAuthService(
            ApplicationDbContext dbContext,
            ISessionCache sessionCache,
            IOptions<OAuthOptions> oauthOptions,
            IOptions<AuthOptions> authOptions,
            IHttpClientFactory httpClientFactory,
            ILogger<ExternalAuthService> logger)
        {
            _dbContext = dbContext;
            _sessionCache = sessionCache;
            _oauthOptions = oauthOptions.Value;
            _authOptions = authOptions.Value;
            _httpClient = httpClientFactory.CreateClient("oauth");
            _logger = logger;
        }

        // ── 1. GENERAR URL DE AUTORIZACIÓN ───────────────────────────────────────

        public async Task<string> GenerateAuthorizationUrlAsync(string provider)
        {
            // Generar state de alta entropía (256 bits) — protección anti-CSRF.
            var state = GenerateSecureToken();

            // Generar code_verifier (RFC 7636 §4.1): 32 bytes aleatorios → Base64URL.
            var codeVerifierBytes = new byte[32];
            RandomNumberGenerator.Fill(codeVerifierBytes);
            var codeVerifier = Base64UrlEncode(codeVerifierBytes);

            // Generar code_challenge = BASE64URL(SHA256(codeVerifier)) — método S256.
            var codeChallenge = ComputeS256Challenge(codeVerifier);

            // Persistir en Redis: key = pkce:state:{state}, value = JSON{challenge, verifier, provider}
            // El verifier se guarda temporalmente aquí; el Launcher también lo guarda en memoria volátil.
            // El backend necesita el challenge para la validación (el verifier viene del Launcher en el callback).
            var pkceData = JsonSerializer.Serialize(new PkceStateData
            {
                CodeChallenge = codeChallenge,
                Provider = provider.ToLowerInvariant()
            });
            await _sessionCache.SetSessionAsync($"{PkcePrefix}{state}", pkceData, PkceTtl);

            _logger.LogInformation("PKCE state generated for provider '{Provider}'. TTL: {Ttl}min", provider, PkceTtl.TotalMinutes);

            return provider.ToLowerInvariant() switch
            {
                "microsoft" => BuildMicrosoftAuthUrl(state, codeChallenge),
                "google" => BuildGoogleAuthUrl(state, codeChallenge),
                _ => throw new ArgumentException($"Unsupported OAuth provider: {provider}")
            };
        }

        // ── 2. PROCESAR CALLBACK ─────────────────────────────────────────────────

        public async Task<ExternalAuthResultDto> ProcessCallbackAsync(OAuthCallbackDto callbackDto)
        {
            var stateKey = $"{PkcePrefix}{callbackDto.State}";
            var replayKey = $"{ReplayPrefix}{callbackDto.Code}";

            // ── ANTI-REPLAY: verificar si el código ya fue usado ─────────────────
            // Directiva 5: Si hay un segundo intento con el mismo código, es un ataque de replay.
            var alreadyUsed = await _sessionCache.GetSessionAsync(replayKey);
            if (alreadyUsed is not null)
            {
                _logger.LogWarning("REPLAY ATTACK DETECTED. Code reuse for provider '{Provider}'.", callbackDto.Provider);
                // Revocar cualquier sesión que el flujo anterior pudo haber emitido.
                // La clave de sesión del primer intento se guarda como valor en replayKey.
                if (alreadyUsed != "pending")
                    await _sessionCache.RemoveSessionAsync(alreadyUsed);

                throw new SecurityException("Authorization code has already been used. Possible replay attack.");
            }

            // Marcar el código como "en proceso" ANTES de intercambiarlo (atómico).
            await _sessionCache.SetSessionAsync(replayKey, "pending", ReplayTtl);

            // ── VALIDACIÓN DEL STATE (anti-CSRF) ─────────────────────────────────
            var pkceJson = await _sessionCache.GetSessionAsync(stateKey);
            if (pkceJson is null)
            {
                _logger.LogWarning("OAuth callback received with unknown or expired state: {State}", callbackDto.State);
                throw new SecurityException("Invalid or expired OAuth state. Possible CSRF attempt.");
            }

            // Destruir el state inmediatamente — un state solo es válido una vez.
            await _sessionCache.RemoveSessionAsync(stateKey);

            var pkceData = JsonSerializer.Deserialize<PkceStateData>(pkceJson)
                           ?? throw new InvalidOperationException("PKCE state data is corrupted.");

            // Verificar que el proveedor del callback coincide con el del state almacenado.
            if (!string.Equals(pkceData.Provider, callbackDto.Provider, StringComparison.OrdinalIgnoreCase))
            {
                throw new SecurityException("Provider mismatch between state and callback.");
            }

            // ── VALIDACIÓN PKCE: code_verifier → S256 → code_challenge ───────────
            // Directiva 3b: El Launcher envía el code_verifier; el servidor lo valida.
            var computedChallenge = ComputeS256Challenge(callbackDto.CodeVerifier);
            if (!CryptographicEquals(computedChallenge, pkceData.CodeChallenge))
            {
                _logger.LogWarning("PKCE code_verifier validation failed for provider '{Provider}'.", callbackDto.Provider);
                throw new SecurityException("PKCE verification failed: code_verifier does not match code_challenge.");
            }

            // ── INTERCAMBIO DE CÓDIGO CON EL PROVEEDOR (server-side) ─────────────
            var profile = callbackDto.Provider.ToLowerInvariant() switch
            {
                "microsoft" => await ExchangeCodeWithMicrosoftAsync(callbackDto.Code),
                "google" => await ExchangeCodeWithGoogleAsync(callbackDto.Code),
                _ => throw new ArgumentException($"Unsupported provider: {callbackDto.Provider}")
            };

            // ── IDENTITY ISOLATION: buscar o crear el Player ──────────────────────
            var authProvider = callbackDto.Provider.ToLowerInvariant() switch
            {
                "microsoft" => AuthProvider.MICROSOFT,
                "google" => AuthProvider.GOOGLE,
                _ => throw new ArgumentException("Unsupported provider.")
            };

            var (player, isNew) = await FindOrCreatePlayerAsync(profile, authProvider);

            // ── EMISIÓN DE SESIÓN COMPATIBLE CON AUTHLIB ─────────────────────────
            // Directiva 6: El token sigue el formato que authlib-injector espera.
            var sessionToken = GenerateSecureToken();
            var sessionTtl = TimeSpan.FromHours(_authOptions.SessionTtlHours);
            await _sessionCache.SetSessionAsync(player.Id.ToString(), sessionToken, sessionTtl);

            // Actualizar la marca anti-replay con el playerId para posible revocación futura.
            await _sessionCache.SetSessionAsync(replayKey, player.Id.ToString(), ReplayTtl);

            _logger.LogInformation(
                "External auth successful. PlayerId: {PlayerId}, Provider: {Provider}, IsNew: {IsNew}",
                player.Id, callbackDto.Provider, isNew);

            return new ExternalAuthResultDto
            {
                PlayerId = player.Id,
                Username = player.Username,
                SessionToken = sessionToken,
                IsNewPlayer = isNew
            };
        }

        // ── FIND OR CREATE PLAYER ─────────────────────────────────────────────────

        private async Task<(Player Player, bool IsNew)> FindOrCreatePlayerAsync(
            ExternalUserProfileDto profile,
            AuthProvider authProvider)
        {
            // Buscar por el ID externo del proveedor en la tabla external_auths.
            var existingAuth = await _dbContext.PlayerExternalAuths
                .Include(ea => ea.Player)
                .FirstOrDefaultAsync(ea => ea.Provider == authProvider && ea.ExternalId == profile.ExternalId);

            if (existingAuth is not null)
            {
                return (existingAuth.Player, false);
            }

            // Nuevo jugador: el UUID se genera en el servidor (Directiva de seguridad global).
            var username = SanitizeUsername(profile.PreferredUsername ?? profile.Email ?? profile.ExternalId);

            // Garantizar unicidad del username si hay colisión.
            if (await _dbContext.Players.AnyAsync(p => p.Username == username))
                username = $"{username}_{Guid.NewGuid().ToString("N")[..6]}";

            var newPlayer = new Player
            {
                Id = Guid.NewGuid(),
                Username = username,
                UuidType = UuidType.PREMIUM, // Cuentas externas usan el tipo PREMIUM (compatible con Mojang ecosystem).
                PasswordHash = null,          // Sin contraseña local: se autentica solo por OAuth.
                CreatedAt = DateTime.UtcNow
            };

            // Inicializar slots de cosméticos vacíos.
            foreach (var slot in Enum.GetValues<CosmeticType>())
            {
                newPlayer.EquippedCosmetics.Add(new PlayerCosmetic
                {
                    PlayerId = newPlayer.Id,
                    Slot = slot,
                    CosmeticId = null,
                    EquippedAt = DateTime.UtcNow
                });
            }

            // Registrar la vinculación con el proveedor externo.
            newPlayer.ExternalAuths.Add(new ExternalAuth
            {
                Id = Guid.NewGuid(),
                PlayerId = newPlayer.Id,
                Provider = authProvider,
                ExternalId = profile.ExternalId,
                Email = profile.Email,
                LinkedAt = DateTime.UtcNow
            });

            _dbContext.Players.Add(newPlayer);
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("New player created via external auth. PlayerId: {PlayerId}, Provider: {Provider}",
                newPlayer.Id, authProvider);

            return (newPlayer, true);
        }

        // ── TOKEN EXCHANGE: MICROSOFT ─────────────────────────────────────────────

        private async Task<ExternalUserProfileDto> ExchangeCodeWithMicrosoftAsync(string code)
        {
            var cfg = _oauthOptions.Microsoft;
            var tokenEndpoint = $"https://login.microsoftonline.com/{cfg.TenantId}/oauth2/v2.0/token";

            var tokenResponse = await _httpClient.PostAsync(tokenEndpoint, new FormUrlEncodedContent(
            [
                new("client_id", cfg.ClientId),
                new("client_secret", cfg.ClientSecret),
                new("code", code),
                new("redirect_uri", cfg.RedirectUri),
                new("grant_type", "authorization_code")
            ]));

            tokenResponse.EnsureSuccessStatusCode();
            var tokenData = await tokenResponse.Content.ReadFromJsonAsync<OAuthTokenResponse>()
                            ?? throw new InvalidOperationException("Microsoft token response was null.");

            // Obtener el perfil del usuario desde Microsoft Graph.
            using var profileRequest = new HttpRequestMessage(HttpMethod.Get, "https://graph.microsoft.com/v1.0/me");
            profileRequest.Headers.Authorization = new("Bearer", tokenData.AccessToken);
            var profileResponse = await _httpClient.SendAsync(profileRequest);
            profileResponse.EnsureSuccessStatusCode();

            var msProfile = await profileResponse.Content.ReadFromJsonAsync<MicrosoftUserProfile>()
                            ?? throw new InvalidOperationException("Microsoft user profile was null.");

            return new ExternalUserProfileDto
            {
                ExternalId = msProfile.Id,
                Email = msProfile.Mail ?? msProfile.UserPrincipalName,
                PreferredUsername = msProfile.DisplayName
            };
        }

        // ── TOKEN EXCHANGE: GOOGLE ────────────────────────────────────────────────

        private async Task<ExternalUserProfileDto> ExchangeCodeWithGoogleAsync(string code)
        {
            var cfg = _oauthOptions.Google;

            var tokenResponse = await _httpClient.PostAsync("https://oauth2.googleapis.com/token",
                new FormUrlEncodedContent(
                [
                    new("client_id", cfg.ClientId),
                    new("client_secret", cfg.ClientSecret),
                    new("code", code),
                    new("redirect_uri", cfg.RedirectUri),
                    new("grant_type", "authorization_code")
                ]));

            tokenResponse.EnsureSuccessStatusCode();
            var tokenData = await tokenResponse.Content.ReadFromJsonAsync<OAuthTokenResponse>()
                            ?? throw new InvalidOperationException("Google token response was null.");

            // Obtener el perfil del usuario desde Google UserInfo.
            using var profileRequest = new HttpRequestMessage(HttpMethod.Get, "https://www.googleapis.com/oauth2/v3/userinfo");
            profileRequest.Headers.Authorization = new("Bearer", tokenData.AccessToken);
            var profileResponse = await _httpClient.SendAsync(profileRequest);
            profileResponse.EnsureSuccessStatusCode();

            var googleProfile = await profileResponse.Content.ReadFromJsonAsync<GoogleUserProfile>()
                                ?? throw new InvalidOperationException("Google user profile was null.");

            return new ExternalUserProfileDto
            {
                ExternalId = googleProfile.Sub,
                Email = googleProfile.Email,
                PreferredUsername = googleProfile.Name
            };
        }

        // ── URL BUILDERS ─────────────────────────────────────────────────────────

        private string BuildMicrosoftAuthUrl(string state, string codeChallenge)
        {
            var cfg = _oauthOptions.Microsoft;
            var baseUrl = $"https://login.microsoftonline.com/{cfg.TenantId}/oauth2/v2.0/authorize";
            var query = new Dictionary<string, string>
            {
                ["client_id"] = cfg.ClientId,
                ["response_type"] = "code",
                ["redirect_uri"] = cfg.RedirectUri,
                ["scope"] = "openid profile email User.Read",
                ["state"] = state,
                ["code_challenge"] = codeChallenge,
                ["code_challenge_method"] = "S256"
            };
            return $"{baseUrl}?{BuildQueryString(query)}";
        }

        private string BuildGoogleAuthUrl(string state, string codeChallenge)
        {
            var cfg = _oauthOptions.Google;
            var query = new Dictionary<string, string>
            {
                ["client_id"] = cfg.ClientId,
                ["response_type"] = "code",
                ["redirect_uri"] = cfg.RedirectUri,
                ["scope"] = "openid profile email",
                ["state"] = state,
                ["code_challenge"] = codeChallenge,
                ["code_challenge_method"] = "S256",
                ["access_type"] = "offline"
            };
            return $"https://accounts.google.com/o/oauth2/v2/auth?{BuildQueryString(query)}";
        }

        // ── UTILIDADES CRIPTOGRÁFICAS ────────────────────────────────────────────

        /// <summary>
        /// Computa el code_challenge S256 = BASE64URL(SHA256(ASCII(code_verifier))).
        /// RFC 7636 §4.2.
        /// </summary>
        private static string ComputeS256Challenge(string codeVerifier)
        {
            var bytes = SHA256.HashData(Encoding.ASCII.GetBytes(codeVerifier));
            return Base64UrlEncode(bytes);
        }

        /// <summary>
        /// Codificación Base64URL (sin padding '=', '+' → '-', '/' → '_').
        /// </summary>
        private static string Base64UrlEncode(byte[] data) =>
            Convert.ToBase64String(data)
                   .TrimEnd('=')
                   .Replace('+', '-')
                   .Replace('/', '_');

        private static string GenerateSecureToken()
        {
            var bytes = new byte[32];
            RandomNumberGenerator.Fill(bytes);
            return Base64UrlEncode(bytes);
        }

        /// <summary>
        /// Comparación de strings en tiempo constante para evitar ataques de timing.
        /// </summary>
        private static bool CryptographicEquals(string a, string b) =>
            CryptographicOperations.FixedTimeEquals(
                Encoding.UTF8.GetBytes(a),
                Encoding.UTF8.GetBytes(b));

        private static string BuildQueryString(Dictionary<string, string> parameters) =>
            string.Join("&", parameters.Select(kv =>
                $"{Uri.EscapeDataString(kv.Key)}={Uri.EscapeDataString(kv.Value)}"));

        private static string SanitizeUsername(string raw) =>
            new string(raw.Where(c => char.IsLetterOrDigit(c) || c == '_' || c == '-').ToArray())
                .ToLowerInvariant()
                .Truncate(32);

        // ── TIPOS INTERNOS (no expuestos fuera de esta clase) ────────────────────

        private sealed record PkceStateData
        {
            public string CodeChallenge { get; init; } = null!;
            public string Provider { get; init; } = null!;
        }

        private sealed class OAuthTokenResponse
        {
            [JsonPropertyName("access_token")]
            public string AccessToken { get; init; } = null!;
        }

        private sealed class MicrosoftUserProfile
        {
            [JsonPropertyName("id")]
            public string Id { get; init; } = null!;

            [JsonPropertyName("displayName")]
            public string? DisplayName { get; init; }

            [JsonPropertyName("mail")]
            public string? Mail { get; init; }

            [JsonPropertyName("userPrincipalName")]
            public string? UserPrincipalName { get; init; }
        }

        private sealed class GoogleUserProfile
        {
            [JsonPropertyName("sub")]
            public string Sub { get; init; } = null!;

            [JsonPropertyName("email")]
            public string? Email { get; init; }

            [JsonPropertyName("name")]
            public string? Name { get; init; }
        }
    }

    // ── EXTENSION METHOD UTILITARIO ───────────────────────────────────────────────

    internal static class StringExtensions
    {
        public static string Truncate(this string value, int maxLength) =>
            value.Length <= maxLength ? value : value[..maxLength];
    }

    // ── EXCEPCIÓN DE SEGURIDAD PERSONALIZADA ──────────────────────────────────────

    public class SecurityException : Exception
    {
        public SecurityException(string message) : base(message) { }
    }
}
