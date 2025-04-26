using NaraChat.Contract.Models.BaseResponse;
using NaraChat.Contract.Models.Chat.Conversation;
using NaraChat.Contract.Models.Users;
using System.Net.Http;
using System.Text.Json;

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
        }
    }

