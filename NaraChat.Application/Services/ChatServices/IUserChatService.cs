using NaraChat.Contract.Models.Users;
using NaraChat.Contract.Models.Users.ServiceViewModels;

namespace NaraChat.Application.Services.ChatServices
{
    public interface IUserChatService
    {
        Task<IEnumerable<UserDto>> GetUsersAsync(string? Search=null,CancellationToken cancellationToken=default);
        Task<IEnumerable<IdViewModel>> GetGroupUsersAsync(string? Search=null,CancellationToken cancellationToken=default);

        Task<TargetUserInfoResponse> GetUserInfo(Guid UserId,CancellationToken cancellationToken=default);
        Task<(bool success, string Message)> UploadNewProfileImage(StreamContent stream,string ContentType,string extension, CancellationToken cancellationToken = default);
        Task<(bool success, string Message)> SubmitBio(string? bio,CancellationToken cancellationToken = default);
        Task<(bool success, string Message)> SubmitEmail(string? email, CancellationToken cancellationToken = default);
        Task<(bool success, string Message)> SubmitPhone(string? phone, CancellationToken cancellationToken = default);
        Task<(bool success, string Message)> SubmitCity(string? city, CancellationToken cancellationToken = default);
        Task<bool> StoreToken(string? token, CancellationToken cancellationToken = default);
        Task<string?> GetAvatar(Guid UserId, CancellationToken cancellationToken = default);


    }

}
