using NaraChat.Contract.Models.Chat.Channels;
using System.Net.Http.Json;
using System;
using NaraChat.Contract.Models.BaseResponse;
using System.Text.Json;
using NaraChat.Contract.Models.Chat.Conversation;
using System.Net.Http;

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
    }
}
