using NaraChat.Contract.Models.BaseResponse;
using NaraChat.Contract.Models.Chat.Channels;
using NaraChat.Contract.Models.Chat.Conversation;
using NaraChat.Contract.Models.Users;
using System;
using System.ComponentModel;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Channels;

namespace NaraChat.Application.Services.ChatServices.Conversation
{
    public class ConversationService(HttpClient client) : IConversationService
    {
        private readonly HttpClient _client = client;



        public async Task<UserAvatar> GetImageConversation(Guid MyUserId, Guid? OtherUserId, CancellationToken cancellationToken = default)
        {
            var result = await _client.PostAsync($"/api/v1/conversation/{MyUserId}/{OtherUserId}/ProcessImage", null);
            if (result.IsSuccessStatusCode)
            {
                var content = await result.Content.ReadAsStringAsync();
                var Response = JsonSerializer.Deserialize<BaseResponseDto<UserAvatar>>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                if (Response?.result is null)
                {
                    return null;
                }
                return Response.result;

            }
            return new UserAvatar();
        }

        public async Task<(bool IsSuccess, string Message)> PinConversation(bool IsPin,Guid ConversationId, CancellationToken cancellationToken = default(CancellationToken))
        {
            try
            {

        
           var result= await _client.PostAsJsonAsync($"/api/v1/conversation/PinConversation",new { IsPin, ConversationId });
            var content = await result.Content.ReadAsStringAsync();
            var Response = JsonSerializer.Deserialize<BaseResponseDto<string>>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            if (Response.status == 200) {
                return (true, Response.message); 
            }
            else {
                return (false, Response.message);

            }
            }
            catch (Exception ex)
            {

                return (false, "عملیات با خطا مواجه شد!!");
            }
        }

        public async Task<PrivateConversationDto?> ReadyOrCreateConversationBy(Guid ToUserId, CancellationToken cancellationToken = default)
        {
            try
            {


                var result = await _client.PostAsync($"/api/v1/conversation/{ToUserId}/CreateConversation", null);
                if (result.IsSuccessStatusCode)
                {
                    var content = await result.Content.ReadAsStringAsync();
                    var Response = JsonSerializer.Deserialize<BaseResponseDto<PrivateConversationDto>>(content,new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    if (Response?.result is null)
                    {
                        return null;
                    }
                    return Response.result;
                }
                return null;
            }
            catch (Exception ex)
            {

                return null;
            }
        }
        public async Task<(bool IsSuccess, string Message)> BlockedConversation(bool IsBlocked, Guid ConversationId, CancellationToken cancellationToken = default)
        {
            try
            {


                var result = await _client.PostAsJsonAsync($"/api/v1/conversation/BlockConversation", new { IsBlock= IsBlocked, ConversationId });
                var content = await result.Content.ReadAsStringAsync();
                var Response = JsonSerializer.Deserialize<BaseResponseDto<string>>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                if (Response.status == 200)
                {
                    return (true, Response.message);
                }
                else
                {
                    return (false, Response.message);

                }
            }
            catch (Exception ex)
            {

                return (false, "عملیات با خطا مواجه شد!!");
            }
        }
        public async Task<(bool IsSuccess, string Message)> CreateGroupConversation(List<Guid> Others, string Title, string UserName, string? Description, CancellationToken cancellationToken = default(CancellationToken))
        {
            try
            {


                var result = await _client.PostAsJsonAsync($"/api/v1/Groups/CreateGroupConversation", new { Others, Title, UserName, Description });
                var content = await result.Content.ReadAsStringAsync();
                var Response = JsonSerializer.Deserialize<BaseResponseDto<string>>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                if (Response.status == 200)
                {
                    return (true, Response.message);
                }
                else
                {
                    return (false, Response.message);

                }
            }
            catch (Exception ex)
            {

                return (false, "عملیات با خطا مواجه شد!!");
            }
        }

        public async Task<List<ChannelMemberViewModel>> GetGroupMembers(Guid coversationId, CancellationToken cancellationToken = default)
        {
            try
            {


                var queryParams = string.Empty;

                var response = await _client.GetFromJsonAsync<BaseResponseDto<List<ChannelMemberViewModel>>>($"/api/v1/Groups/{coversationId}/GroupMembers", new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                if (response?.isSucceded ?? false)
                {

                    return response.result;
                }
                return new List<ChannelMemberViewModel>();
            }
            catch (Exception ex)
            {
                return new List<ChannelMemberViewModel>();

            }
        }

        public async Task<(bool status, string message)> RemoveMemberFromGroup(Guid ConversationId, Guid memberId, CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _client.PostAsJsonAsync($"/api/v1/groups/DeleteMemberFromGroup", new { ConversationId, memberId });
                var content = await result.Content.ReadAsStringAsync();
                var Response = JsonSerializer.Deserialize<BaseResponseDto<string>>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                if (Response.status == 200)
                {
                    return (true, Response.message);
                }
                else
                {
                    return (false, Response.message);

                }
            }
            catch (Exception ex)
            {

                return (false, "عملیات با خطا مواجه شد!!");
            }
        }

        public async Task<(bool issuccess, string message)> PromoteToAdminOrDemoteUser(ChangeUserGroupPolicyCommand command, CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _client.PostAsJsonAsync($"/api/v1/groups/PromotOrDemoteUser", new { command.conversationId, command.UserId, command.ispromote });
                var content = await result.Content.ReadAsStringAsync();
                var Response = JsonSerializer.Deserialize<BaseResponseDto<string>>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                if (Response.status == 200)
                {
                    return (true, Response.message);
                }
                else
                {
                    return (false, Response.message);

                }
            }
            catch (Exception ex)
            {

                return (false, "عملیات با خطا مواجه شد!!");
            }
        }

        public async Task<(bool status, string message)> AddNewMemberByAdmin(AddNewMemberCommand command, CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _client.PostAsJsonAsync($"/api/v1/groups/AddNewMember", new {ConversationId= command.ChannelId, NewUserId= command.MemberId });
                var content = await result.Content.ReadAsStringAsync();
                var Response = JsonSerializer.Deserialize<BaseResponseDto<string>>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                if (Response.status == 200)
                {
                    return (true, Response.message);
                }
                else
                {
                    return (false, Response.message);

                }
            }
            catch (Exception ex)
            {

                return (false, "عملیات با خطا مواجه شد!!");
            }
        }

        public async Task<List<ChatMessageDto>?> LoadGroupMessages(Guid conversationid, int Messagecount = 50, CancellationToken cancellationToken = default)
        {
            try
            {


                var messages = await _client.GetFromJsonAsync<BaseResponseDto<List<ChatMessageDto>?>>($"/api/v1/groups/{conversationid}/messages?messageCount={Messagecount}");
                if (!messages.isSucceded)
                {
                    return null;
                }
                return messages.result;
            }
            catch (Exception)
            {

                return null;
            }
        }

        public async Task<(bool, Guid MessageId,string message)> SendMessageForGroupAsync(Guid ConversationId, string Message, Guid? ParentId = null, float? latitude = null, float? Longitude = null, CancellationToken cancellationToken = default)
        {
            try
            {



                var result = await _client.PostAsJsonAsync($"api/v1/groups/SendMeesageToGroup", new { ConversationId = ConversationId, message = Message, ParentId = ParentId }, new System.Text.Json.JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                });
                var responseApi = JsonSerializer.Deserialize<BaseResponseDto<Guid>>(await result.Content.ReadAsStringAsync());

                if (result.IsSuccessStatusCode)
                {
                    return (true, responseApi!.result,responseApi.message);
                }
                return (false, Guid.Empty, responseApi.message);
            }
            catch (Exception)
            {

                return (false, Guid.Empty,"عملیات با خطا مواجه شد!");
            }
        }

        public async Task<(bool status, string message)> MuteOrUnMuteUser(Guid ConversationId, Guid UserId, bool Mute, CancellationToken cancellationToken = default)
        {
            try
            {


                var result = await _client.PostAsJsonAsync($"/api/v1/Groups/MuteSelectedUser", new { GroupId= ConversationId, UserId= UserId, isMute=Mute });
                var content = await result.Content.ReadAsStringAsync();
                var Response = JsonSerializer.Deserialize<BaseResponseDto<string>>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                if (Response.status == 200)
                {
                    return (true, Response.message);
                }
                else
                {
                    return (false, Response.message);

                }
            }
            catch (Exception ex)
            {

                return (false, "عملیات با خطا مواجه شد!!");
            }
        }

        public async Task<(bool, string message, UploadFileResult? result)> UploadGroupFileAsync(UploadFileDto uploaddto, CancellationToken cancellationToken = default)
        {
            var content = new MultipartFormDataContent();
            uploaddto.file.Headers.ContentType = new MediaTypeHeaderValue(uploaddto.ContentType);

            content.Add(uploaddto.file, "file", uploaddto.fileName);
            content.Add(new StringContent(uploaddto.caption ?? ""), "caption");
            content.Add(new StringContent(uploaddto.ConversationId.ToString()), "conversationId");
            try
            {


                var result = await _client.PostAsync("/api/v1/Groups/UploadFile", content, cancellationToken);
                if (result.IsSuccessStatusCode)
                {
                    var resultapi = await result.Content.ReadAsStringAsync();
                    var FileResult = JsonSerializer.Deserialize<BaseResponseDto<UploadFileResult>>(resultapi, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    return (true, "عملیات با موفقیت انجام شد. ", FileResult!.result);
                }
                return (false, "مشکلی در بارگذاری فایل شما به وجود آمده ", null);
            }
            catch (Exception e)
            {

                return (false, "مشکلی در بارگذاری فایل شما به وجود آمده  لطفا مدتی دیگر مجدد تلاش فرمایید!", null);
            }
        }

   

        public async Task<(bool success, string Message)> UploadGroupAvatarAsync(StreamContent stream, string ContentType, string extension, Guid ChannelId, CancellationToken cancellationToken = default)
        {
            var content = new MultipartFormDataContent();
            stream.Headers.ContentType = new MediaTypeHeaderValue(ContentType);

            content.Add(stream, "file", Guid.NewGuid().ToString() + extension);
            content.Add(new StringContent(ChannelId.ToString()), "GroupId");
            try
            {


                var result = await _client.PostAsync($"/api/v1/groups/SetProfile", content, cancellationToken);
                if (result.IsSuccessStatusCode)
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

        public async Task<List<ChannelFileItem>?> getGroupFiles(Guid groupId, CancellationToken cancellationToken = default)
        {
            var response = await _client.GetFromJsonAsync<BaseResponseDto<List<ChannelFileItem>>>($"/api/v1/groups/getGroupFiles?id={groupId}");
            if (response?.isSucceded ?? false)
            {

                return response.result;
            }
            return null;
        }
        public async Task<string?> downloadGroupFile(Guid fileid, CancellationToken cancellationToken = default)
        {
            var response = await _client.GetFromJsonAsync<BaseResponseDto<string>>($"/api/v1/groups/DownloadGroupFile?id={fileid}");
            if (response?.isSucceded ?? false)
            {

                return response.result;
            }
            return null;
        }
        public async Task<(bool issuccess, string message)> Changebio(ChangeBioGroupCommand command, CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _client.PostAsJsonAsync($"/api/v1/groups/ChangeGroupBio", new { command.bio, command.GroupId });
                var content = await result.Content.ReadAsStringAsync();
                var Response = JsonSerializer.Deserialize<BaseResponseDto<string>>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                if (Response.status == 200)
                {
                    return (true, Response.message);
                }
                else
                {
                    return (false, Response.message);

                }
            }
            catch (Exception ex)
            {

                return (false, "عملیات با خطا مواجه شد!!");
            }
        }
    }
}

