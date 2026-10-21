using Microsoft.AspNetCore.Components.Authorization;
using NaraChat.Application.Services.TokenProvider;
using NaraChat.Contract.Models.Auth;
using NaraChat.Contract.Models.BaseResponse;
using System.Net.Http;
using System.Net.Http.Json;

namespace NaraChat.Application.Services.Auth
{
    public sealed class AuthService(HttpClient httpClient,ITokenService tokenService, AuthenticationStateProvider stateprovider) : IAuthService
    {
        private readonly HttpClient _httpClient= httpClient;
        private readonly ITokenService _tokenService = tokenService;
        private readonly AuthenticationStateProvider _stateprovider = stateprovider;

        public async Task<bool> LoginAsync(LoginDTo loginRequestDto, CancellationToken cancellationToken = default)
        {
            
            var response = await httpClient.PostAsJsonAsync("/api/v1/account/Login", loginRequestDto);
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<BaseResponseDto<TokenDto>>();
                if (result != null)
                {

                    await _tokenService.SetTokenAsync(result.result.token, result.result.refreshtoken);
                    ((CustomAuthenticationStateProvider)_stateprovider).UpdateAuthenticationState();
                    return true;
                }

            }

            return false;
        }

        public async Task<bool> LoginOrRegisterAsync(string phoneNumber,string verifyCode, CancellationToken cancellationToken = default)
        {
            var response = await httpClient.PostAsJsonAsync("/api/v1/users/register-or-login", new { phoneNumber, verifyCode });
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<BaseResponseDto<TokenDto>>();
                if (result != null)
                {
                    if (result.status != 200)
                    {
                        return false;
                    }

                    await _tokenService.SetTokenAsync(result.result.token, result.result.refreshtoken);
                    ((CustomAuthenticationStateProvider)_stateprovider).UpdateAuthenticationState();
                    return true;
                }

            }

            return false;
        }
        public async Task<(bool result, string Message)> SendVerifyCode(string phoneNumber, CancellationToken cancellationToken = default)
        {
            var response = await httpClient.PostAsJsonAsync("/api/v1/users/SendVerifyCode", new { phoneNumber });
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<BaseResponseDto<string?>>();
                if (result != null)
                {
                    if (result.status != 200)
                    {
                        return (false,result.message);
                    }


                    return (true, "کد تایید  ارسال شده به شماره همراه خود را وارد نمایید");
                }

            }

            return (false, "عملیات با خطا مواجه شد");
        }

        public async Task<bool> LoginWithGoogleAsync(string tokenId, CancellationToken cancellationToken = default)
        {

            var response = await httpClient.PostAsJsonAsync("api/v1/account/googleAuthentication", new { idToken= tokenId });
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<BaseResponseDto<TokenDto>>();
                if (result != null)
                {

                    await _tokenService.SetTokenAsync(result.result.token, result.result.refreshtoken);
                    ((CustomAuthenticationStateProvider)_stateprovider).UpdateAuthenticationState();
                    return true;
                }

            }

            return false;
        }

        public async Task<bool> LogoutAsync(CancellationToken cancellationToken = default)
        {
            var refreshToken = await _tokenService.GetReffreshTokenAsync();
            if (string.IsNullOrEmpty(refreshToken))
            {
                throw new InvalidOperationException("RefreshToken Not Found");
            }

            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", await _tokenService.GetAccessTokenAsync());
              var result = await httpClient.PostAsJsonAsync("/api/v1/account/RevokeToken", new { RefreshToken = refreshToken });
            if (result.IsSuccessStatusCode)
            {
                await _tokenService.ClearTokenAsync();
                ((CustomAuthenticationStateProvider)_stateprovider).UpdateAuthenticationState();
                return true;
            }
            else
            {
                await _tokenService.ClearTokenAsync();
                ((CustomAuthenticationStateProvider)_stateprovider).UpdateAuthenticationState();
                return false;
            }
        }

        // فقط یک تازه‌سازی در هر لحظه. وقتی توکن منقضی می‌شود چند درخواست هم‌زمان 401 می‌گیرند؛
        // قبلاً هر کدام جدا refresh می‌کرد، اولی موفق می‌شد و refresh token را عوض می‌کرد، بقیه با
        // توکن قدیمی رد می‌شدند و ClearTokenAsync توکن تازه‌ی اولی را هم پاک می‌کرد → خروج.
        private static readonly SemaphoreSlim RefreshGate = new(1, 1);

        public async Task<string?> RefreshTokenAsync(string refreshToken)
        {
            await RefreshGate.WaitAsync();
            try
            {
                // درخواست دیگری همین الان تازه‌اش کرده؟ همان توکن تازه را بده.
                var current = await _tokenService.GetReffreshTokenAsync();
                if (string.IsNullOrEmpty(current))
                    return null;
                if (current != refreshToken)
                    return await _tokenService.GetAccessTokenAsync();

                HttpResponseMessage response;
                try
                {
                    response = await httpClient.PostAsJsonAsync("/api/v1/account/RefreshToken", new { RefreshToken = refreshToken });
                }
                catch (Exception)
                {
                    // قطعی شبکه یا سرور در حال ری‌استارت: توکن‌ها می‌مانند، دفعه‌ی بعد دوباره امتحان می‌شود.
                    return null;
                }

                if (response.IsSuccessStatusCode)
                {
                    BaseResponseDto<TokenDto>? result = null;
                    try { result = await response.Content.ReadFromJsonAsync<BaseResponseDto<TokenDto>>(); } catch (Exception) { }
                    if (result?.isSucceded == true && !string.IsNullOrEmpty(result.result?.token))
                    {
                        await _tokenService.SetTokenAsync(result.result.token, result.result.refreshtoken);
                        ((CustomAuthenticationStateProvider)_stateprovider).UpdateAuthenticationState();
                        return result.result.token;
                    }
                }

                // فقط وقتی سرور صریحاً گفت refresh token معتبر نیست، کاربر بیرون می‌رود؛ نه با خطای 5xx.
                if (response.IsSuccessStatusCode
                    || response.StatusCode == System.Net.HttpStatusCode.Unauthorized
                    || response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    await _tokenService.ClearTokenAsync();
                    ((CustomAuthenticationStateProvider)_stateprovider).UpdateAuthenticationState();
                }
                return null;
            }
            finally
            {
                RefreshGate.Release();
            }
        }

        /// <summary>
        /// توکن دسترسیِ معتبر برای اتصال‌هایی که خودشان 401 را مدیریت نمی‌کنند (هاب SignalR):
        /// اگر کمتر از یک دقیقه به انقضا مانده، اول تازه می‌شود.
        /// </summary>
        public async Task<string?> GetFreshAccessTokenAsync()
        {
            var token = await _tokenService.GetAccessTokenAsync();
            if (string.IsNullOrEmpty(token))
                return token;

            try
            {
                var jwt = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler().ReadJwtToken(token);
                if (jwt.ValidTo > DateTime.UtcNow.AddMinutes(1))
                    return token;
            }
            catch (Exception)
            {
                return token;
            }

            var refreshToken = await _tokenService.GetReffreshTokenAsync();
            if (string.IsNullOrEmpty(refreshToken))
                return token;
            return await RefreshTokenAsync(refreshToken) ?? await _tokenService.GetAccessTokenAsync();
        }

        public async Task<bool> RegisterAsync(RegisterUserDto command, CancellationToken cancellationToken = default)
        {
            var request = new CreateUserCommand
            {
                Address = new Address(command.City, null, null),
                Age = command.Age,
                FirstName = command.FirstName,
                LastName = command.LastName,
                Gender = command.Gender ? 1 : 2,
                Password = command.Password,
                UserName = command.UserName,
            };

            var response = await _httpClient.PostAsJsonAsync("/api/v1/users/register", request);
            if (response.IsSuccessStatusCode)
            {
         
              


                    return true;
                

            }

            return false;
        }

       
    }
}
