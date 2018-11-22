namespace Galcorp.Auth
{
    public interface IPlatform
    {
        string GetCode(string redirectUri, string authorizationRequest, string state);
        void Output(string s);
        IStore TemporaryStorage { get; }
    }
}