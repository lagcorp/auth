using System;
using System.Collections.Generic;
using System.Text;

namespace Galcorp.Auth
{
    public interface ILoginResult
    {
        bool Success { get; }

        string AccessToken { get;  }
        string IdToken { get;  }
    }
}
