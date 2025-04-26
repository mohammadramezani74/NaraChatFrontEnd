using NaraChat.Contract.Models.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaraChat.Application.Services.Auth
{
    public interface IAuthService
    {
        Task<bool> RegisterAsync(RegisterUserDto command, CancellationToken cancellationToken = default);
        Task<bool> LoginAsync(LoginDTo loginRequestDto, CancellationToken cancellationToken = default);
        Task<bool> LoginOrRegisterAsync(string phoneNumber, string verifyCode, CancellationToken cancellationToken = default);
        Task<(bool result, string Message)> SendVerifyCode(string phoneNumber, CancellationToken cancellationToken = default);
        Task<bool> LoginWithGoogleAsync(string tokenId, CancellationToken cancellationToken = default);
        Task<bool> LogoutAsync( CancellationToken cancellationToken = default);
        Task<string?> RefreshTokenAsync(string refreshToken);
    }
}
