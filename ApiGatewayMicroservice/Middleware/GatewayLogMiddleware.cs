using System;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using MongoDB.Driver;
using Newtonsoft.Json;
using ApiGateway.Models;

namespace ApiGatewayMicroservice.Middleware
{
    public class GatewayLogMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IMongoCollection<RequestLog> _logCollection;

        // Anonim erişime açık yollar
        private static readonly HashSet<string> _anonymousPaths = new(StringComparer.OrdinalIgnoreCase)
        {
            "/api/auth/login",
            "/api/auth/register",
            "/api/auth/refresh-token"
        };

        public GatewayLogMiddleware(RequestDelegate next, IMongoDatabase mongoDatabase)
        {
            _next = next;
            _logCollection = mongoDatabase.GetCollection<RequestLog>("GatewayLogs");
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var stopwatch = Stopwatch.StartNew();

            // 1. CorrelationId
            var correlationId = context.Request.Headers["X-Correlation-Id"].FirstOrDefault();
            if (string.IsNullOrEmpty(correlationId))
            {
                correlationId = Guid.NewGuid().ToString();
                context.Request.Headers["X-Correlation-Id"] = correlationId;
            }

            // 2. JWT & Token Kontrolü
            var token = context.Request.Headers["Authorization"].FirstOrDefault()
                         ?.Replace("Bearer ", "");
            JwtSecurityToken jwt = null;
            bool tokenValid = false;
            string userEmail = null;

            if (!string.IsNullOrEmpty(token))
            {
                try
                {
                    var handler = new JwtSecurityTokenHandler();
                    jwt = handler.ReadJwtToken(token);
                    tokenValid = true;
                    userEmail = jwt?.Claims.FirstOrDefault(c => c.Type == "email")?.Value;
                }
                catch
                {
                    tokenValid = false;
                }
            }

            // 3. Anonim Yol Kontrolü
            bool isAnonymous = _anonymousPaths
                .Any(p => context.Request.Path.StartsWithSegments(p, StringComparison.OrdinalIgnoreCase));

            if (isAnonymous)
                tokenValid = true; // Auth yolları için token gerekmez

            // 4. Downstream'e user email ekle
            context.Request.Headers["X-User-Email"] = userEmail ?? "";

            // 5. Yetkisiz istek ise pipeline devam etmeden logla & döndür
            if (!tokenValid && !isAnonymous)
            {
                var logEntry = new RequestLog
                {
                    Timestamp = DateTime.UtcNow,
                    RequestPath = context.Request.Path,
                    UserEmail = userEmail,
                    CorrelationId = correlationId,
                    Action = "Unauthorized",
                    Message = $"[{context.Request.Method}] {context.Request.Path} → İstek reddedildi: Geçersiz veya eksik JWT. Giriş yapmalısın.",
                };
                await _logCollection.InsertOneAsync(logEntry);
                Console.WriteLine(JsonConvert.SerializeObject(logEntry));
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("Unauthorized: JWT is missing or invalid.");
                return;
            }

            // 6. İşlem devam
            await _next(context);

            stopwatch.Stop();

            // 7. Sonuç & Log
            int statusCode = context.Response.StatusCode;
            string clusterId = context.Request.Headers["X-Cluster-Id"].FirstOrDefault() ?? "UnknownCluster";
            string action;
            string message;

            if (isAnonymous)
            {
                action = "AnonymousAccess";
                message = $"[{context.Request.Method}] {context.Request.Path} → Anonim erişim. Oturum gerekmedi.";
            }
            else if (statusCode >= 200 && statusCode < 300)
            {
                action = "Success";
                message = $"[{context.Request.Method}] {context.Request.Path} → Başarılı istek. Yönlendirildi: {clusterId}. Status: {statusCode}";
            }
            else if (statusCode == 401 || statusCode == 403)
            {
                action = "Forbidden";
                message = $"[{context.Request.Method}] {context.Request.Path} → Yetkisiz erişim. Status: {statusCode}";
            }
            else
            {
                action = "Failed";
                message = $"[{context.Request.Method}] {context.Request.Path} → Hatalı veya reddedildi. Status: {statusCode}";
            }

            var successLog = new RequestLog
            {
                Timestamp = DateTime.UtcNow,
                RequestPath = context.Request.Path,
                UserEmail = userEmail,
                CorrelationId = correlationId,
                Action = action,
                Message = message
            };

            await _logCollection.InsertOneAsync(successLog);
            Console.WriteLine(JsonConvert.SerializeObject(successLog));
        }
    }
}
