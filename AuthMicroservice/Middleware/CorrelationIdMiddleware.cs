using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Serilog.Context;

namespace AuthMicroservice.Middleware
{
    public class CorrelationIdMiddleware
    {
        private const string CorrelationIdHeaderName = "X-Correlation-Id";
        private readonly RequestDelegate _next;

        public CorrelationIdMiddleware(RequestDelegate next) => _next = next;

        public async Task InvokeAsync(HttpContext context)
        {
            var cid = context.Request.Headers[CorrelationIdHeaderName].FirstOrDefault();
            if (string.IsNullOrWhiteSpace(cid))
            {
                cid = Guid.NewGuid().ToString();
                context.Request.Headers[CorrelationIdHeaderName] = cid;
            }

            context.TraceIdentifier = cid;

            Activity.Current ??= new Activity("request");
            Activity.Current.AddTag("correlation-id", cid);

            context.Response.OnStarting(() =>
            {
                context.Response.Headers[CorrelationIdHeaderName] = cid;
                return Task.CompletedTask;
            });

            using (LogContext.PushProperty("CorrelationId", cid))
            {
                await _next(context);
            }
        }
    }
}
