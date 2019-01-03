﻿namespace Galcorp.Auth.UWP
{
    using System.Threading;
    using System.Threading.Tasks;
    using Windows.ApplicationModel.Activation;

    // ReSharper disable once InconsistentNaming
    public class UWPWrapper : IAuthenticationProvider
    {
        private readonly ManualResetEvent _waitForBrowserCallbackEvent = new ManualResetEvent(false);
        private readonly WindowsGoogleClient _wgc;
        private readonly TokenStore _store;

        public UWPWrapper(string clientId, string redirectUri)
        {
            _store = new TokenStore();
            _wgc = new WindowsGoogleClient(clientId, redirectUri, new UWPPlatform());

            AppEventWrapper.ApplicationActivationEvent += AppEventWrapper_ApplicationActivationEvent;
        }

        public GoogleLoginResult AsynchorunsuLoginResult { get; set; }

        public string Name => "Google";

        public async Task<ILoginResult> GetCachedToken()
        {
            return await _store.Read<GoogleLoginResult>( typeof(GoogleLoginResult).FullName );
        }

        public Task<bool> Validate(ILoginResult token)
        {
            return _wgc.LoginTest(token.AccessToken);
        }

        public async Task<ILoginResult> GetToken()
        {
            await _wgc.LoginOpenBrowser();
            _waitForBrowserCallbackEvent.WaitOne();

            return AsynchorunsuLoginResult;
        }

        public ILoginResult RefreshToken(ILoginResult token)
        {
            throw new System.NotImplementedException();
        }

        public Task StoreToken(ILoginResult token)
        {
            return _store.Store(typeof(GoogleLoginResult).FullName, token);
        }

        public Task Logout()
        {
            return _store.Store(typeof(GoogleLoginResult).FullName, null);
        }

        private void AppEventWrapper_ApplicationActivationEvent(IActivatedEventArgs args)
        {
            if (args is ProtocolActivatedEventArgs activation)
            {
                var c = _wgc.HandleIncomingRedirectUri(activation.Uri);
                c.ContinueWith(e =>
                {
                    AsynchorunsuLoginResult = e.Result;
                    _waitForBrowserCallbackEvent.Set();
                });
            }
        }
    }

    public class UWPPlatform : IPlatform
    {
        public string GetCode(string redirectUri, string authorizationRequest, string state)
        {
            throw new System.NotImplementedException();
        }

        public void Output(string s)
        {
            throw new System.NotImplementedException();
        }

        public IStore TemporaryStorage { get; }
    }
}