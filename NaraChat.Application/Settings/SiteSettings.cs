using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaraChat.Application.Settings
{
    public static class SiteSettings
    {

 //public const string ApiUrl = "https://narachatapi.irannara.com/";
      // public const string ApiUrl = "https://intsapitest.irannara.com/";
  public const string ApiUrl = "https://localhost:7194/";
        public const string TokenKey = "_aspNaraToken";
        public const string refreshtokenKey = "_aspnetNaraRefreshToken";
        public const string HubConnectuinUrl = ApiUrl+"hubs/naraHub";

    }
}
