using System;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using Galcorp.Auth.Google;

namespace Galcorp.Demo.Console
{
    using Auth.NetStandard;

    class Program
    {
        // client configuration
        const string clientID = "581786658708-elflankerquo1a6vsckabbhn25hclla0.apps.googleusercontent.com";
        const string clientSecret = "3f6NggMbPtrmIBpgx-MK2xXK";

        static void Main(string[] args)
        {
            var c = new GoogleClient(new Platform());
            var t = c.PerformAuthViaBrowser(clientID, clientSecret).Result;
        }
    }
}
