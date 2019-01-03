namespace Galcorp.Auth.Provider.Google
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net;
    using System.Net.Sockets;
    using System.Security.Cryptography;
    using System.Text;
    using System.Threading.Tasks;
    using Newtonsoft.Json;

    public delegate void WaitForResult();

    public class GoogleClient : IAuthenticationProvider
    {
        private const string AuthorizationEndpoint = "https://accounts.google.com/o/oauth2/v2/auth";
        private readonly string _clientId;
        private readonly IPlatform _platform;
        private readonly string _secret;

        public GoogleClient(IPlatform platform, string clientId, string secret)
        {
            _platform = platform;
            _clientId = clientId;
            _secret = secret;
        }

        public string Name => "Google";

        public Task<ILoginResult> GetToken()
        {
            return PerformAuthViaBrowser(_clientId, _secret);
        }

        public async Task<ILoginResult> GetCachedToken()
        {
            return await _platform.TemporaryStorage.Read<GoogleLoginResult>("gooogle_token");
        }

        public async Task StoreToken(ILoginResult token)
        {
            await _platform.TemporaryStorage.Store("gooogle_token", token);
        }

        public async Task Logout()
        {
            await _platform.TemporaryStorage.Store("gooogle_token", null);
        }

        public async Task<bool> Validate(ILoginResult token)
        {
            return await UserinfoCall(token.AccessToken);
        }

        private async Task<ILoginResult> PerformAuthViaBrowser(string clientId,
            string clientSecret)
        {
            var redirectUri = string.Format("http://{0}:{1}/", IPAddress.Loopback, GetRandomUnusedPort());

            // Generates state and PKCE values.
            var state = RandomDataBase64Url(32);
            var code_verifier = RandomDataBase64Url(32);
            var code_challenge = Base64UrlencodeNoPadding(Sha256(code_verifier));
            const string code_challenge_method = "S256";

            // Creates a redirect URI using an available port on the loopback address.
            _platform.Output("redirect URI: " + redirectUri);

            // Creates the OAuth 2.0 authorization request.
            var authorizationRequest = string.Format(
                "{0}?response_type=code&scope=openid%20profile&redirect_uri={1}&client_id={2}&state={3}&code_challenge={4}&code_challenge_method={5}",
                AuthorizationEndpoint,
                Uri.EscapeDataString(redirectUri),
                clientId,
                state,
                code_challenge,
                code_challenge_method);

            var code = _platform.GetCode(redirectUri, authorizationRequest, state);

            if (!string.IsNullOrWhiteSpace(code))
                return await PerformCodeExchange(code, code_verifier, redirectUri, clientId, clientSecret);

            return new GoogleLoginResult(false);
        }

        private async Task<ILoginResult> PerformCodeExchange(string code, string codeVerifier, string redirectUri,
            string clientId,
            string clientSecret)
        {
            _platform.Output("Exchanging code for tokens...");

            // builds the  request
            var tokenRequestURI = "https://www.googleapis.com/oauth2/v4/token";
            var tokenRequestBody = string.Format(
                "code={0}&redirect_uri={1}&client_id={2}&code_verifier={3}&client_secret={4}&scope=&grant_type=authorization_code",
                code,
                Uri.EscapeDataString(redirectUri),
                clientId,
                codeVerifier,
                clientSecret
            );

            // sends the request
            var tokenRequest = (HttpWebRequest) WebRequest.Create(tokenRequestURI);
            tokenRequest.Method = "POST";
            tokenRequest.ContentType = "application/x-www-form-urlencoded";
            tokenRequest.Accept = "Accept=text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";
            var _byteVersion = Encoding.ASCII.GetBytes(tokenRequestBody);
            tokenRequest.ContentLength = _byteVersion.Length;
            var stream = tokenRequest.GetRequestStream();
            await stream.WriteAsync(_byteVersion, 0, _byteVersion.Length);
            stream.Close();

            try
            {
                // gets the response
                var tokenResponse = await tokenRequest.GetResponseAsync();
                using (var reader = new StreamReader(tokenResponse.GetResponseStream()))
                {
                    // reads response body
                    var responseText = await reader.ReadToEndAsync();
                    Console.WriteLine(responseText);

                    // converts to dictionary
                    var tokenEndpointDecoded = JsonConvert.DeserializeObject<Dictionary<string, string>>(responseText);

                    var access_token = tokenEndpointDecoded["access_token"];
                    var id_token = tokenEndpointDecoded["id_token"];
                    await UserinfoCall(access_token);

                    return new GoogleLoginResult(true)
                    {
                        AccessToken = access_token,
                        IdToken = id_token
                    };
                }
            }
            catch (WebException ex)
            {
                if (ex.Status == WebExceptionStatus.ProtocolError)
                {
                    var response = ex.Response as HttpWebResponse;
                    if (response != null)
                    {
                        _platform.Output("HTTP: " + response.StatusCode);
                        using (var reader = new StreamReader(response.GetResponseStream()))
                        {
                            // reads response body
                            var responseText = await reader.ReadToEndAsync();
                            _platform.Output(responseText);
                        }
                    }
                }
            }

            return new GoogleLoginResult(false);
        }

        private async Task<bool> UserinfoCall(string accessToken)
        {
            _platform.Output("Making API Call to Userinfo...");

            // builds the  request
            var userinfoRequestURI = "https://www.googleapis.com/oauth2/v3/userinfo";

            // sends the request
            var userinfoRequest = (HttpWebRequest) WebRequest.Create(userinfoRequestURI);
            userinfoRequest.Method = "GET";
            userinfoRequest.Headers.Add(string.Format("Authorization: Bearer {0}", accessToken));
            userinfoRequest.ContentType = "application/x-www-form-urlencoded";
            userinfoRequest.Accept = "Accept=text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";

            var result = false;

            try
            {
                var userinfoResponse = await userinfoRequest.GetResponseAsync();
                using (var userinfoResponseReader = new StreamReader(userinfoResponse.GetResponseStream()))
                {
                    // reads response body
                    var userinfoResponseText = await userinfoResponseReader.ReadToEndAsync();
                    _platform.Output(userinfoResponseText);
                    result = true;
                }
            }
            catch (Exception)
            {
                // ignored
            }

            return result;
        }

        /// <summary>
        ///     Returns URI-safe data with a given input length.
        /// </summary>
        /// <param name="length">Input length (nb. output will be longer)</param>
        /// <returns></returns>
        public static string RandomDataBase64Url(uint length)
        {
            var rng = new RNGCryptoServiceProvider();
            var bytes = new byte[length];
            rng.GetBytes(bytes);
            return Base64UrlencodeNoPadding(bytes);
        }

        /// <summary>
        ///     Returns the SHA256 hash of the input string.
        /// </summary>
        /// <param name="inputStirng"></param>
        /// <returns></returns>
        public static byte[] Sha256(string inputStirng)
        {
            var bytes = Encoding.ASCII.GetBytes(inputStirng);
            var sha256 = new SHA256Managed();
            return sha256.ComputeHash(bytes);
        }

        /// <summary>
        ///     Base64url no-padding encodes the given input buffer.
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public static string Base64UrlencodeNoPadding(byte[] buffer)
        {
            var base64 = Convert.ToBase64String(buffer);

            // Converts base64 to base64url.
            base64 = base64.Replace("+", "-");
            base64 = base64.Replace("/", "_");
            // Strips padding.
            base64 = base64.Replace("=", "");

            return base64;
        }

        public static int GetRandomUnusedPort()
        {
            var listener = new TcpListener(IPAddress.Loopback, 0);
            listener.Start();
            var port = ((IPEndPoint) listener.LocalEndpoint).Port;
            listener.Stop();
            return port;
        }
    }
}