using System;

namespace Galcorp.Auth.AzureMobileAppsClient
{
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;

    public class ZumoAuthHeaderHandler : DelegatingHandler
    {
        private readonly string _token;

        public ZumoAuthHeaderHandler(string token)
        {
            _token = token;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(_token))
            {
                throw new InvalidOperationException("User is not currently logged in");
            }

            request.Headers.Add("X-ZUMO-AUTH", _token);
            //request.Headers.Add("ZUMO-API-VERSION", "2.0.0");

            return base.SendAsync(request, cancellationToken);
        }

    }
}
