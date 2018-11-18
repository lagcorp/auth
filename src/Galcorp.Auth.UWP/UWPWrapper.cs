namespace Galcorp.Auth.UWP
{
    using System;
    using System.Diagnostics;
    using System.Threading;
    using System.Threading.Tasks;
    using Windows.ApplicationModel.Activation;
    using Windows.UI.Xaml.Navigation;

    // ReSharper disable once InconsistentNaming
    public class UWPWrapper : IAuthenticationProvider
    {
        private readonly string _clientId;
        private readonly string _redirectUri;
        private WindowsGoogleClient _wgc;
        private readonly ManualResetEvent waitForBrowserCallbackEvent = new ManualResetEvent(false);

        public UWPWrapper(string clientId, string redirectUri)
        {
            _clientId = clientId;
            _redirectUri = redirectUri;

            AppEventWrapper.ApplicationActivationEvent += AppEventWrapper_ApplicationActivationEvent;
        }

        private void AppEventWrapper_ApplicationActivationEvent(Windows.ApplicationModel.Activation.IActivatedEventArgs args)
        {
            if (args is ProtocolActivatedEventArgs activation)
            {
                var c = _wgc.HandleIncomingRedirectUri(activation.Uri);
                c.ContinueWith(e =>
                {
                    this.AsynchorunsuLoginResult = e.Result;
                    waitForBrowserCallbackEvent.Set();
                });
            }
        }

        public GoogleLoginResult AsynchorunsuLoginResult { get; set; }

        public async Task<ILoginResult> Login()
        {
            _wgc = new WindowsGoogleClient(_clientId, _redirectUri);
            await _wgc.LoginOpenBrowser();
            waitForBrowserCallbackEvent.WaitOne();

            return AsynchorunsuLoginResult;
        }
    }
}