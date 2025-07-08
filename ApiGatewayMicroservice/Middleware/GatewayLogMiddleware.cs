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

        public GatewayLogMiddleware(RequestDelegate next, IMongoDatabase mongoDatabase)
        {
            _next = next;
            _logCollection = mongoDatabase.GetCollection<RequestLog>("RequestLogs");
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var stopwatch = Stopwatch.StartNew();

            // 1. CorrelationId kontrol / oluştur
            var correlationId = context.Request.Headers["X-Correlation-Id"].FirstOrDefault();
            if (string.IsNullOrEmpty(correlationId))
            {
                correlationId = Guid.NewGuid().ToString();
                context.Request.Headers["X-Correlation-Id"] = correlationId;
            }

            // 2. Token kontrolü ve JWT bilgileri çıkarma
            var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Replace("Bearer ", "");
            JwtSecurityToken jwt = null;
            bool tokenValid = false;

            if (!string.IsNullOrEmpty(token))
            {
                try
                {
                    var handler = new JwtSecurityTokenHandler();
                    jwt = handler.ReadJwtToken(token);
                    tokenValid = true;
                }
                catch
                {
                    tokenValid = false;
                }
            }

            var userEmail = jwt?.Claims.FirstOrDefault(c => c.Type == "email")?.Value;

            // 3. User bilgilerini downstream servislere header olarak ekle (isteğe bağlı)
            context.Request.Headers["X-User-Email"] = userEmail ?? "";

            // 4. İstek pipeline içinde işleniyor
            await _next(context);

            stopwatch.Stop();

            // 5. Status kodu ve clusterId bilgisi alma (header üzerinden)
            int statusCode = context.Response.StatusCode;
            string clusterId = context.Request.Headers["X-Cluster-Id"].FirstOrDefault() ?? "UnknownCluster";

            // 6. Action ve Message alanlarını duruma göre ayarla
            string action;
            string message;

            if (!tokenValid)
            {
                action = "GeçersizToken";
                message = $"İstek {context.Request.Method} {context.Request.Path} geçersiz veya eksik token nedeniyle reddedildi.";
            }
            else if (statusCode >= 200 && statusCode < 300)
            {
                action = "Yönlendirildi";
                message = $"İstek {context.Request.Method} {context.Request.Path} başarıyla '{clusterId}' cluster'ına yönlendirildi.";
            }
            else if (statusCode == 401 || statusCode == 403)
            {
                action = "Yetkisiz";
                message = $"İstek {context.Request.Method} {context.Request.Path} yetkisiz. Durum kodu: {statusCode}.";
            }
            else
            {
                action = "Reddedildi";
                message = $"İstek {context.Request.Method} {context.Request.Path} hata ile sonuçlandı. Durum kodu: {statusCode}.";
            }

            // 7. Log nesnesi oluştur
            var logEntry = new RequestLog
            {
                Timestamp = DateTime.UtcNow,
                RequestPath = context.Request.Path,
                UserEmail = userEmail,
                CorrelationId = correlationId,
                Action = action,
                Message = message
            };

            // 8. Logu MongoDB'ye kaydet
            await _logCollection.InsertOneAsync(logEntry);

            // 9. Logu konsola yazdır (opsiyonel)
            Console.WriteLine(JsonConvert.SerializeObject(logEntry));
        }
    }
}
