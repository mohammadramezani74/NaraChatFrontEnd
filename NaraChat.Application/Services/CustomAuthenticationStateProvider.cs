using Microsoft.AspNetCore.Components.Authorization;
using NaraChat.Application.Services.TokenProvider;
using NaraChat.Contract.Models.Users;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace NaraChat.Application.Services
{
    public class CustomAuthenticationStateProvider(ITokenService tokenService) : AuthenticationStateProvider
    {
        private readonly ITokenService _tokenService = tokenService;

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var token = await _tokenService.GetAccessTokenAsync();
            if (string.IsNullOrEmpty(token))
            {
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }

            var claims = CreatClaimsFromJwt(token);
            return new AuthenticationState(claims);
        }
        public async Task<string?> GetTokenAsync()
        {
            return  await _tokenService.GetAccessTokenAsync();
        }
        private ClaimsPrincipal CreatClaimsFromJwt(string jwt)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            if (!tokenHandler.CanReadToken(jwt))
            {
                return new ClaimsPrincipal(new ClaimsIdentity());
            }

            var jwtToken = tokenHandler.ReadJwtToken(jwt);
            var claims = jwtToken.Claims;
            var identity = new ClaimsIdentity(claims, "jwt");
            return new ClaimsPrincipal(identity);
        }
        public async Task<UserDto> GetUserInfoAsync()
        {
            var authState = await GetAuthenticationStateAsync();
            var user = authState.User;

            if (user.Identity != null && user.Identity.IsAuthenticated)
            {
                var userId = user.FindFirst("sub")?.Value ?? "Unknown";
                var Name = authState.User.Claims.FirstOrDefault(x=>x.Type=="name")?.Value ?? "Unknown";
                return new UserDto(Guid.Parse(userId), Name);
            }
            return default(UserDto);
        }
        public async Task<bool> IsAuthenticated()
        {
            var authState = await GetAuthenticationStateAsync();

            return authState.User.Identity?.IsAuthenticated??false;

        }
        public void UpdateAuthenticationState()
        {
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }
    }
}
