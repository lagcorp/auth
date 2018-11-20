namespace Galcorp.Auth
{
    using System.Threading.Tasks;

    public class Client
    {
        public Client(IAuthenticationProvider provider)
        {

        }

        public Task Login()
        {
            return Task.CompletedTask;
        }
    }
}