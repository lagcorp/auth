namespace Galcorp.Demo.Console
{
    using Auth.Platform.NetStandard;
    using Auth.Provider.Google;

    internal class Program
    {
        // client configuration
        private const string ClientId = "aaaaaaaaaaaaaaaaa-bbbbbbbbbbbbbbbbbbbb.apps.googleusercontent.com";
        private const string ClientSecret = "cccccccccccccccccccccc";

        private static void Main(string[] args)
        {
            var a = new Auth.Authenticator(
                new GoogleClient(new ConsoleApplication("demo_console"), ClientId, ClientSecret));

            var token = a.Authenticate("google").Result;

        }
    }
}