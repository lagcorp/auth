namespace Galcorp.Auth.UWP
{
    public delegate void LogEntryHandler(object sender, string text);

    // ReSharper disable once InconsistentNaming
    public class UWPPlatform : IPlatform
    {
        private readonly LogEntryHandler _handler;

        public UWPPlatform(LogEntryHandler handler)
        {
            _handler = handler;
            TemporaryStorage = new TokenStore();
        }

        public string GetCode(string redirectUri, string authorizationRequest, string state)
        {
            return "";
        }

        public void Output(string s)
        {
            _handler?.Invoke(this, s);
        }

        public IStore TemporaryStorage { get; }
    }
}