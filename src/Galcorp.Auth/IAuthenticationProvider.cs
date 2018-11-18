namespace Galcorp.Auth
{
    using System.Threading.Tasks;

    public interface IAuthenticationProvider
    {
        Task<ILoginResult> Login();
    }
}