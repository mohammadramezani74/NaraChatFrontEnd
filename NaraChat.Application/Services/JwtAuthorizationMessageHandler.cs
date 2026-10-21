using NaraChat.Application.Services.Auth;
using NaraChat.Application.Services.TokenProvider;
using System.Net.Http.Headers;
using System.Net;
using Microsoft.AspNetCore.Components;


namespace NaraChat.Application.Services
{
    public sealed class JwtAuthorizationMessageHandler : DelegatingHandler
    {
        private readonly ITokenService _tokenService;
        private readonly IAuthService _authService;
        private readonly NavigationManager nm;

        public JwtAuthorizationMessageHandler(ITokenService tokenService, IAuthService authService, NavigationManager nm)
        {
            _tokenService = tokenService;
            _authService = authService;
            this.nm = nm;
        }
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var token = await _tokenService.GetAccessTokenAsync();
            if (!string.IsNullOrEmpty(token))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
            var response = await base.SendAsync(request, cancellationToken);
            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                var refreshToken = await _tokenService.GetReffreshTokenAsync();
                if (!string.IsNullOrEmpty(refreshToken))
                {
                    var newToken = await _authService.RefreshTokenAsync(refreshToken);
                    if (!string.IsNullOrEmpty(newToken))
                    {
                        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", newToken);
                        return await base.SendAsync(request, cancellationToken);
                    }
                }
                // فقط وقتی واقعاً از حساب بیرون رفته (توکن‌ها پاک شده) به صفحه‌ی ورود برو؛
                // اگر تازه‌سازی به‌خاطر قطعی شبکه نشد، کاربر نباید بیرون بیفتد.
                if (string.IsNullOrEmpty(await _tokenService.GetAccessTokenAsync()))
                    nm.NavigateTo("/", true);
            }
            return response;
        }
    }
}
