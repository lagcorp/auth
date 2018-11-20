namespace Galcorp.Auth
{
    using System.Threading.Tasks;

    public interface IAuthenticationProvider
    {
        string Name { get; }
        Task<ILoginResult> GetToken();
    }
}