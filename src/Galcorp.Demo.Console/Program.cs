namespace Galcorp.Demo.Console
{
    using Auth.Platform.NetStandard;
    using Auth.Provider.Google;

    public partial class Program
    {
        private static void Main(string[] args)
        {
            var a = new Auth.Authenticator(
                new GoogleClient(new ConsoleApplication("demo_console"), ClientId, ClientSecret));

            var token = a.Authenticate("google").Result;

        }
    }
}