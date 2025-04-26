using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaraChat.Application.Services.TokenProvider
{
    public  interface ITokenService
    {
        Task<string?> GetAccessTokenAsync();
        Task<string?> GetReffreshTokenAsync();
        Task SetTokenAsync(string AccessToken, string RefreshToken);
        Task ClearTokenAsync();
    }
}
