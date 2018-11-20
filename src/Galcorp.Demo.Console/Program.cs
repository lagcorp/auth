namespace Galcorp.Demo.Console
{
    using Auth.Google;
    using Auth.Platform.NetStandard;

    internal class Program
    {
        // client configuration
        private const string ClientId = "581786658708-elflankerquo1a6vsckabbhn25hclla0.apps.googleusercontent.com";
        private const string ClientSecret = "3f6NggMbPtrmIBpgx-MK2xXK";

        private static void Main(string[] args)
        {
            var a = new Auth.Authenticator();
            a.Register(new GoogleClient(new Platform(), ClientId, ClientSecret));
        }
    }
}