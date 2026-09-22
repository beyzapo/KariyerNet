using System.Net;
using KariyerNet.Application.Exceptions;

namespace KariyerNet.API.Middleware
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Not found: {Path}", context.Request.Path);
                await WriteResponseAsync(context, HttpStatusCode.NotFound, ex.Message);
            }
            catch (AuthenticationFailedException ex)
            {
                _logger.LogWarning(ex, "Authentication failed: {Path}", context.Request.Path);
                await WriteResponseAsync(context, HttpStatusCode.Unauthorized, ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized: {Path}", context.Request.Path);
                await WriteResponseAsync(context, HttpStatusCode.Forbidden, ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Conflict: {Path}", context.Request.Path);
                await WriteResponseAsync(context, HttpStatusCode.Conflict, ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception: {Path}", context.Request.Path);
                await WriteResponseAsync(context, HttpStatusCode.InternalServerError, "Beklenmeyen bir hata oluştu.");
            }
        }

        private static Task WriteResponseAsync(HttpContext context, HttpStatusCode statusCode, string message)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)statusCode;
            return context.Response.WriteAsJsonAsync(new { message });
        }
    }
}
