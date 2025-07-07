using ApiGateway.Models;
using ApiGateway.Services;
using Microsoft.AspNetCore.Http;
using System.Diagnostics;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Security.Claims;


namespace ApiGateway.Middleware
{
    public class LoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly LogService _logService;
        private readonly IMongoDatabase _mongoDatabase;


        public LoggingMiddleware(RequestDelegate next, LogService logService, IMongoDatabase mongoDatabase)
        {
            _next = next;
            _logService = logService;
            _mongoDatabase = mongoDatabase;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var sw = Stopwatch.StartNew();
            string? errorMessage = null;

            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                sw.Stop(); // burada durdur
                errorMessage = ex.ToString();
                Console.WriteLine($"Loglama sırasında hata: {errorMessage}");

                // Logu hemen yaz
                await LogRequestAsync(context, sw.ElapsedMilliseconds, errorMessage);

                throw; // throw'u geri koy, böylece servis doğru şekilde hata fırlatabilir
            }

            sw.Stop();
            await LogRequestAsync(context, sw.ElapsedMilliseconds, null);
        }

        private async Task LogRequestAsync(HttpContext context, long elapsedMs, string? errorMessage)
        {
            var statusCode = context.Response.StatusCode;
            var userId = context.User?.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

            var logDocument = new BsonDocument
            {
                { "Timestamp", DateTime.UtcNow },
                { "Method", context.Request.Method },
                { "Path", context.Request.Path.ToString() },
                { "StatusCode", statusCode },
                { "IP", context.Connection.RemoteIpAddress?.MapToIPv4().ToString() ?? "Unknown" },
                //{ "UserId", userId != null ? (BsonValue)userId : BsonNull.Value },
                { "ServiceName", DetermineServiceName(context.Request.Path) },
                { "ResponseTimeMs", elapsedMs }
            };

            if (!string.IsNullOrEmpty(userId))
            {
                logDocument.Add("UserId", userId);
            }


            if (string.IsNullOrWhiteSpace(errorMessage) && statusCode >= 400)
            {
                errorMessage = $"Request failed with status code {statusCode}";
            }

                if (!string.IsNullOrWhiteSpace(errorMessage))
            {
                logDocument.Add("ErrorMessage", errorMessage);
            }

            var collection = _mongoDatabase.GetCollection<BsonDocument>("RequestLogs");
            await collection.InsertOneAsync(logDocument);
        }


        private string DetermineServiceName(string path)
        {
            if (path.StartsWith("/api/auth", StringComparison.OrdinalIgnoreCase)) return "AuthService";
            if (path.StartsWith("/api/products", StringComparison.OrdinalIgnoreCase)) return "ProductService";
            if (path.StartsWith("/api/address", StringComparison.OrdinalIgnoreCase)) return "AddressService";
            if (path.StartsWith("/api/paymenttype", StringComparison.OrdinalIgnoreCase)) return "PaymentTypeService";
            if (path.StartsWith("/api/productcategory", StringComparison.OrdinalIgnoreCase)) return "ProductCategoryService";
            if (path.StartsWith("/api/productconfiguration", StringComparison.OrdinalIgnoreCase)) return "ProductConfigurationService";
            if (path.StartsWith("/api/productitem", StringComparison.OrdinalIgnoreCase)) return "ProductItemService";
            if (path.StartsWith("/api/shippingtype", StringComparison.OrdinalIgnoreCase)) return "ShippingTypeService";
            if (path.StartsWith("/api/shoporder", StringComparison.OrdinalIgnoreCase)) return "ShopOrderService";
            if (path.StartsWith("/api/shoppingcart", StringComparison.OrdinalIgnoreCase)) return "ShoppingCartService";
            if (path.StartsWith("/api/useraddress", StringComparison.OrdinalIgnoreCase)) return "UserAddressService";
            if (path.StartsWith("/api/user", StringComparison.OrdinalIgnoreCase)) return "UserService";
            if (path.StartsWith("/api/variationoption", StringComparison.OrdinalIgnoreCase)) return "VariationOptionService";
            if (path.StartsWith("/api/variation", StringComparison.OrdinalIgnoreCase)) return "VariationService";
            if (path.StartsWith("/api/orderstatus", StringComparison.OrdinalIgnoreCase)) return "OrderStatusService";

            return "Unknown";
        }

    }
}
