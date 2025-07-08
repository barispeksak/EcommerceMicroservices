using System;                     // Guid
using System.Diagnostics;         // Activity
using System.Linq;                // FirstOrDefault
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Serilog.Context;

namespace VariationOptionMicroservice.Middleware
{
    public class CorrelationIdMiddleware
    {
        private const string CorrelationIdHeaderName = "X-Correlation-Id";
        private readonly RequestDelegate _next;

        public CorrelationIdMiddleware(RequestDelegate next) => _next = next;

        public async Task InvokeAsync(HttpContext context)
        {
            // 1. Read incoming header or create a new one
            var cid = context.Request.Headers[CorrelationIdHeaderName].FirstOrDefault();
            if (string.IsNullOrWhiteSpace(cid))
            {
                cid = Guid.NewGuid().ToString();
                context.Request.Headers[CorrelationIdHeaderName] = cid;
            }

            // 2. Publish the id to all logging / tracing mechanisms
            context.TraceIdentifier = cid;

            Activity.Current ??= new Activity("request");
            Activity.Current.AddTag("correlation-id", cid);

            // 3. Return the id in the response header
            context.Response.OnStarting(() =>
            {
                context.Response.Headers[CorrelationIdHeaderName] = cid;
                return Task.CompletedTask;
            });

            // 4. Run the rest of the pipeline inside a Serilog scope
            using (LogContext.PushProperty("CorrelationId", cid))
            {
                await _next(context);
            }
        }

        // (optional helpers – safe to remove if unused)
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