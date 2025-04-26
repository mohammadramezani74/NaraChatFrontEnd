
using NaraChat.Application.Services.LocalStorage;
using NaraChat.Application.Settings;

namespace NaraChat.Application.Services.TokenProvider
{
    public class TokenService(ILocalStorageService localStorage) : ITokenService
    {
        private readonly ILocalStorageService _localStorage = localStorage;

        public async Task ClearTokenAsync()
        {
            await _localStorage.RemoveItemAsync(SiteSettings.TokenKey);
            await _localStorage.RemoveItemAsync(SiteSettings.refreshtokenKey);
        }

        public async Task<string?> GetAccessTokenAsync()
        {
            return await _localStorage.GetItemAsync(SiteSettings.TokenKey);
        }

        public async Task<string?> GetReffreshTokenAsync()
        {
            return await _localStorage.GetItemAsync(SiteSettings.refreshtokenKey);
        }

        public async Task SetTokenAsync(string AccessToken, string RefreshToken)
        {
            await _localStorage.SetItemAsync(SiteSettings.TokenKey, AccessToken);
            await _localStorage.SetItemAsync(SiteSettings.refreshtokenKey, RefreshToken);
        }
    }
}
