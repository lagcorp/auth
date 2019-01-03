namespace Galcorp.Demo.Console
{
    using Auth.Platform.NetStandard;
    using Auth.Provider.Google;

    internal class Program
    {
        // client configuration
        private const string ClientId = "581786658708-elflankerquo1a6vsckabbhn25hclla0.apps.googleusercontent.com";
        private const string ClientSecret = "3f6NggMbPtrmIBpgx-MK2xXK";

        private static void Main(string[] args)
        {
            var a = new Auth.Authenticator(
                new GoogleClient(new Platform("demo_console"), ClientId, ClientSecret));

            var token = a.Authenticate("google").Result;

        }
    }
}