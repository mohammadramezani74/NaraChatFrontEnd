using NaraChat.Contract.Models.BaseResponse;
using NaraChat.Contract.Models.Users;
using NaraChat.Contract.Models.Users.ServiceViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Reflection.Metadata;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace NaraChat.Application.Services.ChatServices
{
    public sealed class UserChatService: IUserChatService
    {
        private readonly HttpClient _httpClient;

        public UserChatService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }



        public async Task<IEnumerable<UserDto>> GetUsersAsync(string? Search=null,CancellationToken cancellationToken = default)
        {
            try
            {

         
            var queryParams = string.Empty;
            if (!string.IsNullOrWhiteSpace(Search))
            {
                queryParams += $"?search={Search}";
            }
            var response = await _httpClient.GetFromJsonAsync<BaseResponseDto<List<GetUsersViewModel>>>("/api/v1/users/GetAll", new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            if (response?.isSucceded??false) {
                var users=response.result.Select(x=>new UserDto(x.Id,x.LastName+" "+x.FirstName,x.Avatar,messageUnreadedCount:x.MessageUnreadedCount,lastseen:x.LastSeen,lastReceivedMessage: x.LastReceivedMessage,
                   lastReceivedMessageId: x.LastReceivedMessageId,isLastReceivedMessageForMe: x.IsLastReceivedMessageForMe,lastReceivedMessageSendDate:x.LastReceivedMessageSendDate,
                   isPinned:x.IsPin,isBlocked:x.IsBlocked,conversationId:x.ConversationId,otherUserBlocked:x.OtherUserBlocked,ischannel: x.IsChannel
                   ,chanel:x.channel
                   )).ToList();
                return users;
            }
            return Enumerable.Empty<UserDto>();
            }
            catch (Exception ex)
            {
                return Enumerable.Empty<UserDto>();

            }

        }
        public async Task<TargetUserInfoResponse> GetUserInfo(Guid UserId, CancellationToken cancellationToken = default)
        {
            var response = await _httpClient.GetFromJsonAsync<BaseResponseDto<TargetUserInfoResponse>>($"/api/v1/users/GetGetUserBy?id={UserId}");
            if (response?.isSucceded ?? false)
            {
           
                return response.result;
            }
            return new TargetUserInfoResponse(); ;
        }
        public async Task<string?> GetAvatar(Guid UserId, CancellationToken cancellationToken = default)
        {
            var response = await _httpClient.GetFromJsonAsync<BaseResponseDto<CurrentUserAvatarResponse>>($"/api/v1/users/getavatar?id={UserId}");
            if (response?.isSucceded ?? false)
            {

                return response.result.picture;
            }
            return null ;
        }

        public async Task<(bool success,string Message)> UploadNewProfileImage(StreamContent stream, string ContentType,string extension, CancellationToken cancellationToken = default)
        {
            var content = new MultipartFormDataContent();
            stream.Headers.ContentType=new MediaTypeHeaderValue(ContentType);
       
            content.Add(stream, "file",Guid.NewGuid().ToString()+extension);
            try
            {

       
            var result = await _httpClient.PostAsync("/api/v1/users/User/SetProfile", content,cancellationToken);
               if(result.IsSuccessStatusCode)
                {
              
                    var avatarPath = JsonSerializer.Deserialize<BaseResponseDto<string>>(await result.Content.ReadAsStringAsync());
                    return (true, avatarPath.result);
                }
                return (false, "مشکلی در تغییر عکس به وجود آمده ");
            }
            catch (Exception e)
            {

                return (false, "مشکلی در تغییر عکس به وجود آمده ");
            }
        }

        public async Task<(bool success, string Message)> SubmitBio(string? bio, CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _httpClient.PostAsJsonAsync("/api/v1/users/ModifiedUser", new { bio = bio });
                if (result.IsSuccessStatusCode)
                {
                    return (true, bio ?? "هنوز چیزی ننوشته اید");
                }
                return (false, "مشکلی در ثبت بیو به وجود آمده لطفا بعدا مراجعه فرمایید!");

            }
            catch (Exception)
            {

                return (false, "مشکلی در ثبت بیو به وجود آمده لطفا بعدا مراجعه فرمایید!");
            }
        
        }
        public async Task<bool> StoreToken(string? token, CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _httpClient.PostAsJsonAsync("/api/v1/conversation/StoreFCMToken", new { token = token });
                if (result.IsSuccessStatusCode)
                {
                    return true;
                }
                return false;

            }
            catch (Exception ex)
            {

                return false;
            }

        }
        public async Task<(bool success, string Message)> SubmitEmail(string? email, CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _httpClient.PostAsJsonAsync("/api/v1/users/ModifiedUser", new { email = email });
                if (result.IsSuccessStatusCode)
                {
                    return (true, email ?? "هنوز چیزی ننوشته اید");
                }
                return (false, "مشکلی در ثبت ایمیل به وجود آمده لطفا بعدا مراجعه فرمایید!");

            }
            catch (Exception)
            {

                return (false, "مشکلی در ثبت ایمیل به وجود آمده لطفا بعدا مراجعه فرمایید!");
            }

        }
        public async Task<(bool success, string Message)> SubmitPhone(string? phone, CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _httpClient.PostAsJsonAsync("/api/v1/users/ModifiedUser", new { phoneNumber = phone });
                if (result.IsSuccessStatusCode)
                {
                    return (true, phone ?? "هنوز چیزی ننوشته اید");
                }
                return (false, "مشکلی در ثبت شماره همراه به وجود آمده لطفا بعدا مراجعه فرمایید!");

            }
            catch (Exception)
            {

                return (false, "مشکلی در شماره همراه به وجود آمده لطفا بعدا مراجعه فرمایید!");
            }

        }
        public async Task<(bool success, string Message)> SubmitCity(string? city, CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _httpClient.PostAsJsonAsync("/api/v1/users/ModifiedUser", new { city = city });
                if (result.IsSuccessStatusCode)
                {
                    return (true, city ?? "هنوز چیزی ننوشته اید");
                }
                return (false, "مشکلی در ثبت شهر به وجود آمده لطفا بعدا مراجعه فرمایید!");

            }
            catch (Exception)
            {

                return (false, "مشکلی در ثبت شهر به وجود آمده لطفا بعدا مراجعه فرمایید!");
            }

        }
    }
}
