using IceBackend.Api.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace IceBackend.UnitTests.Middlewares
{
    public class ExceptionHandlingMiddlewareTests
    {
        private readonly Mock<ILogger<ExceptionHandlingMiddleware>> _loggerMock;
        private readonly Mock<IHostEnvironment> _envMock;
        private readonly DefaultHttpContext _context;

        public ExceptionHandlingMiddlewareTests()
        {
            _loggerMock = new Mock<ILogger<ExceptionHandlingMiddleware>>();
            _envMock = new Mock<IHostEnvironment>();
            _context = new DefaultHttpContext();
            _context.Response.Body = new MemoryStream();
        }

        [Fact]
        public async Task InvokeAsync_InvalidOperationException_Returns400_AndTruncatesSensitiveDataInDevelopment()
        {
            // Arrange
            _envMock.Setup(e => e.EnvironmentName).Returns("Development");
            
            var exceptionToThrow = new InvalidOperationException("Failed because Database ConnectionString is invalid and Password was wrong.");
            
            RequestDelegate next = (HttpContext ctx) => throw exceptionToThrow;

            var middleware = new ExceptionHandlingMiddleware(next, _loggerMock.Object, _envMock.Object);

            // Act
            await middleware.InvokeAsync(_context);

            // Assert
            Assert.Equal(StatusCodes.Status400BadRequest, _context.Response.StatusCode);
            
            _context.Response.Body.Seek(0, SeekOrigin.Begin);
            var responseBody = await new StreamReader(_context.Response.Body).ReadToEndAsync();
            
            using var doc = JsonDocument.Parse(responseBody);
            var root = doc.RootElement;
            
            var detail = root.GetProperty("detail").GetString();
            Assert.NotNull(detail);
            Assert.Equal("[Redacted: Sensitive information removed from stack trace.]", detail);
            Assert.DoesNotContain("ConnectionString", detail);
            Assert.DoesNotContain("Password", detail);
        }

        [Fact]
        public async Task InvokeAsync_SecurityException_Returns403()
        {
            // Arrange
            _envMock.Setup(e => e.EnvironmentName).Returns("Production");
            var exceptionToThrow = new System.Security.SecurityException("CSRF failed.");
            RequestDelegate next = (HttpContext ctx) => throw exceptionToThrow;
            var middleware = new ExceptionHandlingMiddleware(next, _loggerMock.Object, _envMock.Object);

            // Act
            await middleware.InvokeAsync(_context);

            // Assert
            Assert.Equal(StatusCodes.Status403Forbidden, _context.Response.StatusCode);
        }
    }
}
