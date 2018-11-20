namespace Galcorp.Auth.Provider.Google
{
    public class GoogleLoginResult : ILoginResult
    {
        public GoogleLoginResult(bool success)
        {
            Success = success;
        }

        public bool Success { get; }
        public string AccessToken { get; set; }
        public string IdToken { get; set; }
    }
}