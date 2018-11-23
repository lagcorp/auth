using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

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

        public async Task<ILoginResult> Authenticate(string provider)
        {
            var p = GetProvider(provider);

            ILoginResult token = await p.GetCachedToken();
            if(token!=null)
            {
                if(!Validate(token))
                {
                    token = p.RefreshToken(token);
                    if(token.Success)
                    {
                        await p.StoreToken(token);
                    }
                }
            }else{
                token = await p.GetToken();
                if(token.Success)
                {
                    if(token.Success)
                    {
                        await p.StoreToken(token);
                    }
                }
            }

            return token;
        }

        private bool Validate(ILoginResult token)
        {
            return true;
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
            this._providers.Add(provider);
        }
    }
}