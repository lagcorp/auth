namespace Galcorp.Auth.UWP
{
    using System.Threading;
    using System.Threading.Tasks;
    using Windows.ApplicationModel.Activation;

    // ReSharper disable once InconsistentNaming
    public class UWPWrapper : IAuthenticationProvider
    {
        private readonly string _clientId;
        private readonly string _redirectUri;
        private readonly ManualResetEvent _waitForBrowserCallbackEvent = new ManualResetEvent(false);
        private WindowsGoogleClient _wgc;

        public UWPWrapper(string clientId, string redirectUri)
        {
            _clientId = clientId;
            _redirectUri = redirectUri;

            AppEventWrapper.ApplicationActivationEvent += AppEventWrapper_ApplicationActivationEvent;
        }

        public GoogleLoginResult AsynchorunsuLoginResult { get; set; }

        public async Task<ILoginResult> Login()
        {
            _wgc = new WindowsGoogleClient(_clientId, _redirectUri);
            await _wgc.LoginOpenBrowser();
            _waitForBrowserCallbackEvent.WaitOne();

            return AsynchorunsuLoginResult;
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
}