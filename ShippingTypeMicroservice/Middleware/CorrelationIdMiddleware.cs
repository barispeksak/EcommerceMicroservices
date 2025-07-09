using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Serilog.Context;

namespace ShippingTypeMicroservice.Middleware
{
    public class CorrelationIdMiddleware
    {
        private const string CorrelationIdHeaderName = "X-Correlation-Id";
        private readonly RequestDelegate _next;

        public CorrelationIdMiddleware(RequestDelegate next) => _next = next;

        public async Task InvokeAsync(HttpContext context)
        {
            // 1. Header’dan correlation-id al ya da üret
            var cid = context.Request.Headers[CorrelationIdHeaderName].FirstOrDefault();
            if (string.IsNullOrWhiteSpace(cid))
            {
                cid = Guid.NewGuid().ToString();
                context.Request.Headers[CorrelationIdHeaderName] = cid;
            }

            // 2. Trace ve logging için id’yi yay
            context.TraceIdentifier = cid;

            Activity.Current ??= new Activity("request");
            Activity.Current.AddTag("correlation-id", cid);

            // 3. Yanıtta correlation-id header olarak geri dön
            context.Response.OnStarting(() =>
            {
                context.Response.Headers[CorrelationIdHeaderName] = cid;
                return Task.CompletedTask;
            });

            // 4. Serilog scope'u içinde devam et
            using (LogContext.PushProperty("CorrelationId", cid))
            {
                await _next(context);
            }
        }

        // (isteğe bağlı yardımcılar – kullanmıyorsan silebilirsin)
        private static string? GetCorrelationIdFromHeader(HttpContext context) =>
            context.Request.Headers.TryGetValue(CorrelationIdHeaderName, out var v) &&
            !string.IsNullOrWhiteSpace(v) ? v.ToString() : null;

        private static void AddCorrelationIdToResponse(HttpContext context, string cid) =>
            context.Response.OnStarting(() =>
            {
                context.Response.Headers[CorrelationIdHeaderName] = cid;
                return Task.CompletedTask;
            });
    }
}
