https://github.com/Azure/azure-mobile-apps-net-client/issues/78#issuecomment-186289700

You can create a delegating handler for add this header request.

public class ZumoAuthHeaderHandler : DelegatingHandler
{
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(AccountService.Instance.AuthenticationToken))
            {
                throw new InvalidOperationException("User is not currently logged in");
            }

            request.Headers.Add("X-ZUMO-AUTH", AccountService.Instance.AuthenticationToken);
            request.Headers.Add("ZUMO-API-VERSION", "2.0.0");

            return base.SendAsync(request, cancellationToken);
        }
}
and use like this:

using (var handler = new ZumoAuthHeaderHandler ()) 
{
                using (var client = MobileServiceClientFactory.CreateClient (handler)) 
                {
                }
}
I don't know if this is the best solution but it works

remiroyc commented on 19 Feb 2016
https://github.com/Azure/azure-mobile-apps-net-client/issues/78