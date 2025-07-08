using Serilog.Context;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace UserMicroservice.Middleware
{
    public class CorrelationIdMiddleware
    {
        private readonly RequestDelegate _next;
        private const string CorrelationIdHeaderName = "X-Correlation-ID";

        public CorrelationIdMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var correlationId = GetCorrelationIdFromHeader(context);
            if (string.IsNullOrEmpty(correlationId))
            {
                // İstersen buraya log koyabilirsin, ama YENİ ID OLUŞTURMA!
                correlationId = "missing-correlation-id"; 
            }

            AddCorrelationIdToResponse(context, correlationId);

            using (LogContext.PushProperty("CorrelationId", correlationId))
            {
                await _next(context);
            }
        }

        private static string GetCorrelationIdFromHeader(HttpContext context)
        {
            if (context.Request.Headers.TryGetValue(CorrelationIdHeaderName, out var correlationId))
            {
                if (!string.IsNullOrWhiteSpace(correlationId))
                    return correlationId.ToString();
            }
            return null;
        }

        private static void AddCorrelationIdToResponse(HttpContext context, string correlationId)
        {
            context.Response.OnStarting(() =>
            {
                context.Response.Headers[CorrelationIdHeaderName] = correlationId;
                return Task.CompletedTask;
            });
        }
    }
}
