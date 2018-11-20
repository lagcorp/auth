namespace Galcorp.Auth
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public class Authenticator
    {
        readonly List<IAuthenticationProvider> _providers = new List<IAuthenticationProvider>();

        public Authenticator(params IAuthenticationProvider[] providers)
        {
            this._providers.AddRange(providers);
        }

        public Task<ILoginResult> GetToken(string provider)
        {
            return _providers.Single(p => string.Equals(p.Name, provider, StringComparison.InvariantCultureIgnoreCase))
                .GetToken();
        }

        public void Register(IAuthenticationProvider provider)
        {
            this._providers.Add(provider);
        }
    }
}