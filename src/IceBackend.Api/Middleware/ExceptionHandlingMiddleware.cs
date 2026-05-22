using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Security;
using System.Text.Json;
using System.Threading.Tasks;
using System.Linq;

namespace IceBackend.Api.Middleware
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;
        private readonly IHostEnvironment _env;

        private static readonly string[] SensitiveKeywords = new[] { "ConnectionString", "Database", "Password", "Secret" };

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger, IHostEnvironment env)
        {
            _next = next;
            _logger = logger;
            _env = env;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unhandled exception has occurred while executing the request.");
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var problemDetails = new ProblemDetails
            {
                Instance = context.Request.Path,
                Title = "An error occurred while processing your request.",
                Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1"
            };

            problemDetails.Extensions["traceId"] = context.TraceIdentifier;

            switch (exception)
            {
                case ArgumentNullException _:
                case ArgumentException _:
                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                    problemDetails.Title = "Bad Request";
                    problemDetails.Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1";
                    break;
                case SecurityException _:
                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    problemDetails.Title = "Forbidden";
                    problemDetails.Type = "https://tools.ietf.org/html/rfc7231#section-6.5.3";
                    break;
                case InvalidOperationException _:
                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                    problemDetails.Title = "Bad Request";
                    problemDetails.Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1";
                    break;
                default:
                    context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                    break;
            }

            problemDetails.Status = context.Response.StatusCode;

            if (_env.IsDevelopment())
            {
                var detail = exception.ToString();
                if (ContainsSensitiveInformation(detail))
                {
                    detail = "[Redacted: Sensitive information removed from stack trace.]";
                }
                problemDetails.Detail = detail;
            }

            context.Response.ContentType = "application/problem+json";
            
            var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            await context.Response.WriteAsync(JsonSerializer.Serialize(problemDetails, options));
        }

        private bool ContainsSensitiveInformation(string detail)
        {
            if (string.IsNullOrEmpty(detail))
                return false;

            return SensitiveKeywords.Any(keyword => detail.Contains(keyword, StringComparison.OrdinalIgnoreCase));
        }
    }
}
