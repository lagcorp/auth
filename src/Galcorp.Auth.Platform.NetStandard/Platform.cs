namespace Galcorp.Auth.Platform.NetStandard
{
    using System;
    using System.Diagnostics;
    using System.Net;
    using System.Runtime.InteropServices;
    using System.Text;

    public class Platform : IPlatform
    {
        public string GetCode(string redirectUri, string authorizationRequest, string state)
        {
            var http = CreateHttpListner(redirectUri);

            OpenBrowser(authorizationRequest);

            var context = http.GetContext();
            var code = ExtractCode(context, state);

            // Sends an HTTP response to the browser.
            WriteResponse(context, http);
            return code;
        }

        public void Output(string listening)
        {
            Console.WriteLine(listening);
        }

        public IStore TemporaryStorage
        {
            get { return new TemporaryStorage(); }
        }

        private HttpListener CreateHttpListner(string redirectUri)
        {
            var http = new HttpListener();
            http.Prefixes.Add(redirectUri);
            Output("Listening..");
            http.Start();
            return http;
        }

        private static void OpenBrowser(string url)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                url = url.Replace("&", "^&");
                Process.Start(new ProcessStartInfo("cmd", $"/c start {url}"));
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                Process.Start("xdg-open", url);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                Process.Start("open", url);
            }
            else
            {
                throw new NotSupportedException("Unable to open browser on this platform.");
            }
        }

        private string ExtractCode(HttpListenerContext context, string state)
        {
            string code = null;

            // Checks for errors.
            if (context.Request.QueryString.Get("error") != null)
            {
                Output(string.Format("OAuth authorization error: {0}.", context.Request.QueryString.Get("error")));
                return null;
            }

            if (context.Request.QueryString.Get("code") == null
                || context.Request.QueryString.Get("state") == null)
            {
                Output("Malformed authorization response. " + context.Request.QueryString);
                return null;
            }

            // extracts the code
            code = context.Request.QueryString.Get("code");
            var incoming_state = context.Request.QueryString.Get("state");

            // Compares the receieved state to the expected value, to ensure that
            // this app made the request which resulted in authorization.
            if (incoming_state != state)
            {
                Output(string.Format("Received request with invalid state ({0})", incoming_state));
                return null;
            }

            Output("Authorization code: " + code);
            return code;
        }

        private static void WriteResponse(HttpListenerContext context, HttpListener http)
        {
            var response = context.Response;
            var responseString =
                "<html><head><meta http-equiv=\'refresh\' content=\'10;url=https://google.com\'></head><body>Please return to the app.</body></html>";
            var buffer = Encoding.UTF8.GetBytes(responseString);
            response.ContentLength64 = buffer.Length;
            var responseOutput = response.OutputStream;
            var responseTask = responseOutput.WriteAsync(buffer, 0, buffer.Length).ContinueWith(task =>
            {
                responseOutput.Close();
                http.Stop();
                Console.WriteLine("HTTP server stopped.");
            });
        }
    }
}