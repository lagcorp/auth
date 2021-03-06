﻿namespace Galcorp.Auth
{
    using System.Threading.Tasks;

    public interface IAuthenticationProvider
    {
        string Name { get; }
        Task<ILoginResult> GetToken();
        Task<ILoginResult> GetCachedToken();
        Task<bool> Validate(ILoginResult token);
        Task StoreToken(ILoginResult token);
        Task Logout();
    }
}