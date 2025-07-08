using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace VariationOptionMicroservice.Http
{
    public class CorrelationIdDelegatingHandler : DelegatingHandler
    {
        private const string CorrelationIdHeaderName = "X-Correlation-Id";
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CorrelationIdDelegatingHandler(IHttpContextAccessor httpContextAccessor) =>
            _httpContextAccessor = httpContextAccessor;

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
                                                                     CancellationToken cancellationToken)
        {
            var cid = _httpContextAccessor.HttpContext?
                                         .Request?
                                         .Headers[CorrelationIdHeaderName]
                                         .FirstOrDefault();

            if (!string.IsNullOrWhiteSpace(cid) &&
                !request.Headers.Contains(CorrelationIdHeaderName))
            {
                request.Headers.Add(CorrelationIdHeaderName, cid);
            }

            return await base.SendAsync(request, cancellationToken);
        }
    }
}