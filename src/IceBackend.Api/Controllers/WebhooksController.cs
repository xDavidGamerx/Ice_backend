using IceBackend.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace IceBackend.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WebhooksController : ControllerBase
    {
        private readonly IStripeWebhookValidator _validator;
        private readonly IStripeWebhookService _webhookService;
        private readonly ILogger<WebhooksController> _logger;

        public WebhooksController(
            IStripeWebhookValidator validator,
            IStripeWebhookService webhookService,
            ILogger<WebhooksController> logger)
        {
            _validator = validator;
            _webhookService = webhookService;
            _logger = logger;
        }

        /// <summary>
        /// Endpoint receptor de webhooks de Stripe.
        /// Lee el raw body manualmente para poder validar la firma criptográfica.
        /// Directiva 5: Siempre retorna 200 OK tras validar la firma, para evitar reintentos de Stripe.
        /// </summary>
        [HttpPost("stripe")]
        public async Task<IActionResult> StripeWebhook()
        {
            // Leer el body crudo como string — [FromBody] NO puede usarse porque
            // Stripe necesita el payload exacto sin modificaciones para verificar el HMAC-SHA256.
            string rawBody;
            using (var reader = new StreamReader(Request.Body, leaveOpen: true))
            {
                rawBody = await reader.ReadToEndAsync();
            }

            var signatureHeader = Request.Headers["Stripe-Signature"].FirstOrDefault();

            if (string.IsNullOrEmpty(signatureHeader))
            {
                _logger.LogWarning("Webhook received without Stripe-Signature header.");
                return BadRequest("Missing Stripe-Signature header.");
            }

            // ── VALIDACIÓN CRIPTOGRÁFICA EN EL BORDE ──────────────────────────────
            // Si la firma no es válida, rechazar aquí. Ninguna capa de negocio es tocada.
            var webhookEvent = _validator.ValidateAndConstruct(rawBody, signatureHeader);

            if (webhookEvent is null)
            {
                _logger.LogWarning("Webhook rejected: invalid signature.");
                return BadRequest("Invalid Stripe signature.");
            }

            // ── RESPUESTA INMEDIATA: Directiva 5 ──────────────────────────────────
            // Disparar el procesamiento en background y responder 200 OK de inmediato.
            // Esto evita que Stripe agote su timeout (30s) y reintente el evento.
            _ = Task.Run(async () =>
            {
                try
                {
                    await _webhookService.HandleEventAsync(webhookEvent);
                }
                catch (Exception ex)
                {
                    // El error queda logueado; Stripe reintentará si necesario.
                    _logger.LogError(ex, "Background webhook processing failed for EventId: {EventId}", webhookEvent.EventId);
                }
            });

            return Ok();
        }
    }
}
