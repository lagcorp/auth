namespace Galcorp.Auth
{
    using System.Threading.Tasks;

    public interface IAuthenticationProvider
    {
        string Name { get; }
        Task<ILoginResult> GetToken();
        Task<ILoginResult> GetCachedToken();
        bool Validate(ILoginResult token);
        ILoginResult RefreshToken(ILoginResult token);
        Task StoreToken(ILoginResult token);
    }
}