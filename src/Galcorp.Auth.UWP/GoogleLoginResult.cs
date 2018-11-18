namespace Galcorp.Auth.UWP
{
    public class GoogleLoginResult : ILoginResult
    {
        public GoogleLoginResult(bool success)
        {
            Success = success;
        }

        public bool Success { get; }
        public string Bearer { get; set; }
    }
}