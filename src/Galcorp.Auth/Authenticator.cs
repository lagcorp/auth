namespace Galcorp.Auth
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public class Authenticator
    {
        private readonly List<IAuthenticationProvider> _providers = new List<IAuthenticationProvider>();

        /// <summary>
        /// Authenticator wrapper to allow many auth providers
        /// </summary>
        /// <param name="providers"></param>
        public Authenticator(params IAuthenticationProvider[] providers)
        {
            _providers.AddRange(providers);
        }

        public async Task<ILoginResult> Authenticate(string provider)
        {
            var p = GetProvider(provider);

            var token = await p.GetCachedToken();
            if (token != null)
            {
                if (!await Validate(provider, token))
                {
                    token = await p.GetToken();
                    if (token.Success) await p.StoreToken(token);
                }
            }
            else
            {
                token = await p.GetToken();
                if (token.Success)
                    if (token.Success)
                        await p.StoreToken(token);
            }

            return token;
        }

        private Task<bool> Validate(string provider, ILoginResult token)
        {
            var p = GetProvider(provider);

            return p.Validate(token);
        }

        public Task<ILoginResult> GetToken(string provider)
        {
            return GetProvider(provider).GetToken();
        }

        private IAuthenticationProvider GetProvider(string provider)
        {
            return _providers.Single(p => string.Equals(p.Name, provider, StringComparison.InvariantCultureIgnoreCase));
        }

        public void Register(IAuthenticationProvider provider)
        {
            _providers.Add(provider);
        }
    }
}