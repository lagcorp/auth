namespace Galcorp.Auth.UWP
{
    using System;
    using System.Diagnostics;
    using System.Linq;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Text;
    using System.Threading.Tasks;
    using Windows.Data.Json;
    using Windows.Security.Cryptography;
    using Windows.Security.Cryptography.Core;
    using Windows.Storage;
    using Windows.Storage.Streams;
    using Windows.System;

    public class WindowsGoogleClient
    {
        private const string AuthorizationEndpoint = "https://accounts.google.com/o/oauth2/v2/auth";
        private const string TokenEndpoint = "https://www.googleapis.com/oauth2/v4/token";
        private const string UserInfoEndpoint = "https://www.googleapis.com/oauth2/v3/userinfo";
        private readonly string _clientId;
        private readonly string _redirectUri;

        public WindowsGoogleClient(string clientId, string redirectUri)
        {
            _clientId = clientId;
            _redirectUri = redirectUri;
        }


        public async Task LoginOpenBrowser()
        {
            var state = RandomDataBase64Url(32);
            var code_verifier = RandomDataBase64Url(32);
            var code_challenge = Base64UrlencodeNoPadding(Sha256(code_verifier));
            const string code_challenge_method = "S256";

            // Stores the state and code_verifier values into local settings.
            // Member variables of this class may not be present when the app is resumed with the
            // authorization response, so LocalSettings can be used to persist any needed values.
            var localSettings = ApplicationData.Current.LocalSettings;
            localSettings.Values["state"] = state;
            localSettings.Values["code_verifier"] = code_verifier;

            // Creates the OAuth 2.0 authorization request.
            var authorizationRequest = string.Format(
                "{0}?response_type=code&scope=openid%20profile&redirect_uri={1}&client_id={2}&state={3}&code_challenge={4}&code_challenge_method={5}",
                AuthorizationEndpoint,
                Uri.EscapeDataString(_redirectUri),
                _clientId,
                state,
                code_challenge,
                code_challenge_method);

            Output("Opening authorization request URI: " + authorizationRequest);

            // Opens the Authorization URI in the browser.
            var success = await Launcher.LaunchUriAsync(new Uri(authorizationRequest));
        }

        /// <summary>
        ///     Appends the given string to the on-screen log, and the debug console.
        /// </summary>
        /// <param name="output">string to be appended</param>
        private void Output(string output)
        {
            Debug.WriteLine(output);
        }

        /// <summary>
        ///     Returns URI-safe data with a given input length.
        /// </summary>
        /// <param name="length">Input length (nb. output will be longer)</param>
        /// <returns></returns>
        private static string RandomDataBase64Url(uint length)
        {
            var buffer = CryptographicBuffer.GenerateRandom(length);
            return Base64UrlencodeNoPadding(buffer);
        }

        /// <summary>
        ///     Returns the SHA256 hash of the input string.
        /// </summary>
        /// <param name="inputStirng"></param>
        /// <returns></returns>
        private static IBuffer Sha256(string inputStirng)
        {
            var sha = HashAlgorithmProvider.OpenAlgorithm(HashAlgorithmNames.Sha256);
            var buff = CryptographicBuffer.ConvertStringToBinary(inputStirng, BinaryStringEncoding.Utf8);
            return sha.HashData(buff);
        }

        private static string Base64UrlencodeNoPadding(IBuffer buffer)
        {
            var base64 = CryptographicBuffer.EncodeToBase64String(buffer);

            // Converts base64 to base64url.
            base64 = base64.Replace("+", "-");
            base64 = base64.Replace("/", "_");
            // Strips padding.
            base64 = base64.Replace("=", "");

            return base64;
        }

        /// <summary>
        ///     Handles redirect after browser call
        /// </summary>
        /// <param name="authorizationResponse"></param>
        public async Task<GoogleLoginResult> HandleIncomingRedirectUri(Uri authorizationResponse)
        {
            var queryString = authorizationResponse.Query;
            Output("MainPage received authorizationResponse: " + authorizationResponse);

            // Parses URI params into a dictionary
            // ref: http://stackoverflow.com/a/11957114/72176
            var queryStringParams =
                queryString.Substring(1).Split('&')
                    .ToDictionary(c => c.Split('=')[0],
                        c => Uri.UnescapeDataString(c.Split('=')[1]));

            if (queryStringParams.ContainsKey("error"))
            {
                Output(string.Format("OAuth authorization error: {0}.", queryStringParams["error"]));
                return new GoogleLoginResult(false);
            }

            if (!queryStringParams.ContainsKey("code")
                || !queryStringParams.ContainsKey("state"))
            {
                Output("Malformed authorization response. " + queryString);
                return new GoogleLoginResult(false);
            }

            // Gets the Authorization code & state
            var code = queryStringParams["code"];
            var incoming_state = queryStringParams["state"];

            // Retrieves the expected 'state' value from local settings (saved when the request was made).
            var localSettings = ApplicationData.Current.LocalSettings;
            var expected_state = (string) localSettings.Values["state"];

            // Compares the receieved state to the expected value, to ensure that
            // this app made the request which resulted in authorization
            if (incoming_state != expected_state)
            {
                Output(string.Format("Received request with invalid state ({0})", incoming_state));
                return new GoogleLoginResult(false);
            }

            // Resets expected state value to avoid a replay attack.
            localSettings.Values["state"] = null;

            // Authorization Code is now ready to use!
            Output(Environment.NewLine + "Authorization code: " + code);

            var code_verifier = (string) localSettings.Values["code_verifier"];
            return await PerformCodeExchangeAsync(code, code_verifier);

            
        }

        private async Task<GoogleLoginResult> PerformCodeExchangeAsync(string code, string code_verifier)
        {
            // Builds the Token request
            var tokenRequestBody = string.Format(
                "code={0}&redirect_uri={1}&client_id={2}&code_verifier={3}&scope=&grant_type=authorization_code",
                code,
                Uri.EscapeDataString(_redirectUri),
                _clientId,
                code_verifier
            );
            var content = new StringContent(tokenRequestBody, Encoding.UTF8, "application/x-www-form-urlencoded");

            // Performs the authorization code exchange.
            var handler = new HttpClientHandler();
            handler.AllowAutoRedirect = true;
            var client = new HttpClient(handler);

            Output(Environment.NewLine + "Exchanging code for tokens...");
            var response = await client.PostAsync(TokenEndpoint, content);
            var responseString = await response.Content.ReadAsStringAsync();
            Output(responseString);

            if (!response.IsSuccessStatusCode)
            {
                Output("Authorization code exchange failed.");
                return new GoogleLoginResult(false);
            }

            // Sets the Authentication header of our HTTP client using the acquired access token.
            var tokens = JsonObject.Parse(responseString);
            var accessToken = tokens.GetNamedString("access_token");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            // Makes a call to the Userinfo endpoint, and prints the results.
            Output("Making API Call to Userinfo...");
            var userinfoResponse = client.GetAsync(UserInfoEndpoint).Result;
            var userinfoResponseContent = await userinfoResponse.Content.ReadAsStringAsync();

            Output(userinfoResponseContent);

            return new GoogleLoginResult(true)
            {
                Bearer = accessToken
            };
        }
    }
}