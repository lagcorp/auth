namespace Galcorp.Auth.AzureMobileAppsClient
{
    using Microsoft.WindowsAzure.MobileServices;

    public class Caller
    {
        public static MobileServiceClient GetClient(
            string url,
            string beararToken)
        {
            var handler = new ZumoAuthHeaderHandler(beararToken);
            return new MobileServiceClient(url, handler);
        }
    }
}