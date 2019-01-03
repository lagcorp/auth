namespace Galcorp.Auth
{
    public interface IPlatform
    {
        /// Get code, use browser for that purpose and result it, 
        /// this code should be used for asking for permanat token
        string GetCode(string redirectUri, string authorizationRequest, string state);
        void Output(string s);
        IStore TemporaryStorage { get; }
    }
}