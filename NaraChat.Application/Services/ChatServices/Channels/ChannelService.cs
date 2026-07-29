using NaraChat.Contract.Models.Chat.Channels;
using System.Net.Http.Json;
using System;
using NaraChat.Contract.Models.BaseResponse;
using System.Text.Json;
using NaraChat.Contract.Models.Chat.Conversation;
using System.Net.Http;
using NaraChat.Contract.Models.Users;
using NaraChat.Contract.Models.Users.ServiceViewModels;
using System.Threading.Channels;
using System.Net.Http.Headers;

namespace NaraChat.Application.Services.ChatServices.Channels
{
    public class ChannelService(HttpClient client) : IChannelService
    {
        private readonly HttpClient _client = client;



        public async Task<(bool issuccess, string message)> CreateChannel(CreateChannelCommand command, CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _client.PostAsJsonAsync($"/api/v1/channel/CreatenewChannel", new { command.Title, command.Description, command.UserName, command.IsPublic });
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

        public async Task<(bool issuccess, string message)> ChangeDescription(ChangeBioChannelCommand command, CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _client.PostAsJsonAsync($"/api/v1/channel/ChangeDescription", new { command.bio, command.ChannelId });
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
        public async Task<(bool issuccess, string message)> PromoteToAdminOrDemoteUser(ChangeUserChannelPolicyCommand command, CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _client.PostAsJsonAsync($"/api/v1/channel/PromotOrDemoteUser", new { command.channelId, command.UserId,command.ispromote });
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
        public async Task<(bool issuccess, string message)> JoinToPublicChannel(Guid ChannelId, CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _client.PostAsJsonAsync($"/api/v1/channel/JoinPublicChannel", new { ChannelId });
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
        public async Task<List<ChatMessageDto>?> LoadChannelMessages(Guid channelId, int Messagecount = 50, CancellationToken cancellationToken = default)
        {
            try
            {


                var messages = await _client.GetFromJsonAsync<BaseResponseDto<List<ChatMessageDto>?>>($"/api/v1/channel/{channelId}/messages?messageCount={Messagecount}");
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

        public async Task<(bool, Guid MessageId)> SendMessageForChannelAsync(Guid ChannelId, string Message, Guid? ParentId = null, float? latitude = null, float? Longitude = null, CancellationToken cancellationToken = default)
        {
            try
            {



                var result = await _client.PostAsJsonAsync($"api/v1/channel/messages", new { ChannelId = ChannelId, message = Message, ParentId = ParentId }, new System.Text.Json.JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                });
                if (result.IsSuccessStatusCode)
                {
                    var responseApi = JsonSerializer.Deserialize<BaseResponseDto<Guid>>(await result.Content.ReadAsStringAsync());
                    return (true, responseApi!.result);
                }
                return (false, Guid.Empty);
            }
            catch (Exception)
            {

                return (false, Guid.Empty);
            }
        }

        public async Task<List<UserDto>> GetPublicChannels(CancellationToken cancellationToken = default)
        {
            try
            {


                var queryParams = string.Empty;
           
                var response = await _client.GetFromJsonAsync<BaseResponseDto<List<GetUsersViewModel>>>("/api/v1/channel/publicChannels", new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                if (response?.isSucceded ?? false)
                {
                    var users = response.result.Select(x => new UserDto(x.Id, x.LastName + " " + x.FirstName, x.Avatar, messageUnreadedCount: x.MessageUnreadedCount, lastseen: x.LastSeen, lastReceivedMessage: x.LastReceivedMessage,
                       lastReceivedMessageId: x.LastReceivedMessageId, isLastReceivedMessageForMe: x.IsLastReceivedMessageForMe, lastReceivedMessageSendDate: x.LastReceivedMessageSendDate,
                       isPinned: x.IsPin, isBlocked: x.IsBlocked, conversationId: x.ConversationId, otherUserBlocked: x.OtherUserBlocked, ischannel: x.IsChannel
                       , chanel: x.channel, username: x.UserName
                       )).ToList();
                    return users;
                }
                return new List<UserDto>();
            }
            catch (Exception ex)
            {
                return new List<UserDto>();

            }
        }

        public async Task<(bool status, string message)> AddNewMemberByAdmin(AddNewMemberCommand command, CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _client.PostAsJsonAsync($"/api/v1/channel/AddNewMember", new { command.ChannelId,command.MemberId });
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

        public async Task<List<ChannelMemberViewModel>> GetChannelMembers(Guid channelid, CancellationToken cancellationToken = default)
        {
            try
            {


                var queryParams = string.Empty;

                var response = await _client.GetFromJsonAsync<BaseResponseDto<List<ChannelMemberViewModel>>>($"/api/v1/channel/{channelid}/ChannelMembers", new JsonSerializerOptions
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
        public async Task<(bool success, string Message)> UploadNewProfileImage(StreamContent stream, string ContentType, string extension,Guid ChannelId, CancellationToken cancellationToken = default)
        {
            var content = new MultipartFormDataContent();
            stream.Headers.ContentType = new MediaTypeHeaderValue(ContentType);

            content.Add(stream, "file", Guid.NewGuid().ToString() + extension);
            content.Add(new StringContent(ChannelId.ToString()), "channelId");
            try
            {


                var result = await _client.PostAsync($"/api/v1/channel/SetProfile", content, cancellationToken);
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
        public async Task<string?> GetAvatar(Guid UserId, CancellationToken cancellationToken = default)
        {
            var response = await _client.GetFromJsonAsync<BaseResponseDto<CurrentUserAvatarResponse>>($"/api/v1/channel/getavatar?id={UserId}");
            if (response?.isSucceded ?? false)
            {

                return response.result.picture;
            }
            return null;
        }
        public async Task<string?> downloadChannelFile(Guid fileid, CancellationToken cancellationToken = default)
        {
            var response = await _client.GetFromJsonAsync<BaseResponseDto<string>>($"/api/v1/channel/DownloadChannelQuery?id={fileid}");
            if (response?.isSucceded ?? false)
            {

                return response.result;
            }
            return null;
        }
        public async Task<List<ChannelFileItem>?> getChannelFiles(Guid channelId, CancellationToken cancellationToken = default)
        {
            var response = await _client.GetFromJsonAsync<BaseResponseDto<List<ChannelFileItem>>>($"/api/v1/channel/getChannelFiles?id={channelId}");
            if (response?.isSucceded ?? false)
            {

                return response.result;
            }
            return null;
        }

        public async Task<(bool status,string message)> RemoveMemberFromChannel(Guid memberid, Guid channelid, CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _client.PostAsJsonAsync($"/api/v1/channel/DeleteMemberFromChannel", new { memberid, channelid });
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
        public async Task<(bool status, string message)> ClearChannelHistory(
         Guid channelId, CancellationToken cancellationToken = default)
         => await SendDelete($"/api/v1/channel/{channelId}/history", cancellationToken);

        public async Task<(bool status, string message)> DeleteChannel(
            Guid channelId, CancellationToken cancellationToken = default)
            => await SendDelete($"/api/v1/channel/{channelId}", cancellationToken);
        private async Task<(bool status, string message)> SendDelete(
    string url, CancellationToken cancellationToken)
        {
            try
            {
                var result = await _client.DeleteAsync(url, cancellationToken);
                var content = await result.Content.ReadAsStringAsync(cancellationToken);

                var response = JsonSerializer.Deserialize<BaseResponseDto<string>>(
                    content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                return response?.status == 200
                    ? (true, response.message)
                    : (false, response?.message ?? "عملیات ناموفق بود.");
            }
            catch (Exception)
            {
                return (false, "خطا در ارتباط با سرور.");
            }
        }

    }
}
