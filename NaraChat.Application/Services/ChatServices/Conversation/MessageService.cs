using Microsoft.AspNetCore.Authorization.Infrastructure;
using NaraChat.Contract.Models.BaseResponse;
using NaraChat.Contract.Models.Chat.Conversation;
using System;
using System.IO;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Net.Mime;
using System.Reflection.Metadata;
using System.Text.Json;

namespace NaraChat.Application.Services.ChatServices.Conversation
{
    public class MessageService(HttpClient httpClient) : IMessageService
    {
        private readonly HttpClient _httpClient = httpClient;

        public async Task<SearchPageDto?> SearchMessages(
    Guid? conversationId,
    Guid? channelId,
    string term,
    DateTime? before = null,
    int take = 20,
    CancellationToken cancellationToken = default)
        {
            try
            {
                var url = $"/api/v1/message/search?q={Uri.EscapeDataString(term)}&take={take}";

                if (conversationId.HasValue) url += $"&conversationId={conversationId}";
                if (channelId.HasValue) url += $"&channelId={channelId}";
                if (before.HasValue) url += $"&before={Uri.EscapeDataString(before.Value.ToString("O"))}";

                var response = await _httpClient
                    .GetFromJsonAsync<BaseResponseDto<SearchPageDto>>(url, cancellationToken);

                return response?.isSucceded == true ? response.result : null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<List<ChatMessageDto>?> LoadMessagesAround(
            Guid messageId,
            int take = 20,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await _httpClient
                    .GetFromJsonAsync<BaseResponseDto<List<ChatMessageDto>>>(
                        $"/api/v1/message/{messageId}/around?take={take}", cancellationToken);

                return response?.isSucceded == true ? response.result : null;
            }
            catch (Exception)
            {
                return null;
            }
        }
        public async Task<MessagesPageDto?> LoadMessages(
     Guid conversationId,
     DateTime? before = null,
     int take = 30,
     CancellationToken cancellationToken = default)
        {
            try
            {
                var url = $"/api/v1/message/{conversationId}/messages?take={take}";

                // فرمت "O" رفت‌وبرگشت دقیق دارد و چون CreateDate در سرور Unspecified
                // است، آفستی اضافه نمی‌شود و ساعت جابه‌جا نمی‌شود.
                if (before.HasValue)
                    url += $"&before={Uri.EscapeDataString(before.Value.ToString("O"))}";

                var response = await _httpClient
                    .GetFromJsonAsync<BaseResponseDto<MessagesPageDto>>(url, cancellationToken);

                return response?.isSucceded == true ? response.result : null;
            }
            catch (Exception)
            {
                return null;
            }
        }
        public async Task<(bool,Guid MessageId)> SendMessageAsync(Guid ConversationId, string Message, Guid? ParentId = null, float? latitude = null, float? Longitude = null, CancellationToken cancellationToken = default)
        {
            try
            {

         

            var result = await _httpClient.PostAsJsonAsync($"api/v1/message/messages", new { conversationId= ConversationId, message= Message, ParentId=ParentId, latitude=latitude, Longitude=Longitude },new System.Text.Json.JsonSerializerOptions
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

        public async Task<(bool, string message)> EditMessageAsync(EditedMessageDto editedMessage, CancellationToken cancellationToken = default)
        {
            try
            {

        
            var result = await _httpClient.PutAsJsonAsync($"api/v1/message/EditMessage", new {MessageId= editedMessage.Id,Message= editedMessage.Message, otherUserId=editedMessage.OtherId }, new System.Text.Json.JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
            });
            result.EnsureSuccessStatusCode();
                return (true, "ویرایش پیام با موفقیت انجام شد!");
            }
            catch (Exception ex)
            {

                return (false, "ویرایش پیام به خطا مواجه شد!");
            }
        }

        public async Task<(bool, string message)> DeleteMessageAsync(Guid MessageId, Guid OtherId, CancellationToken cancellationToken = default)
        {
            try
            {

         
            var result = await _httpClient.DeleteAsync($"/api/v1/message/{MessageId}/{OtherId}/DeleteMessage");
                result.EnsureSuccessStatusCode();
                return (true, "حذف پیام با موفقیت انجام شد.");
            }
            catch (Exception)
            {

                return (false, "حذف پیام با خطا مواجه شد!");
            }

        }

        public async Task<(bool, string message, UploadFileResult? result)> UploadChatFileAsync(UploadFileDto uploaddto, CancellationToken cancellationToken = default)
        {
            var content = new MultipartFormDataContent();
            uploaddto.file.Headers.ContentType = new MediaTypeHeaderValue(uploaddto.ContentType);
    
            content.Add(uploaddto.file, "file",uploaddto.fileName);
            content.Add(new StringContent(uploaddto.caption ?? ""), "caption");
            content.Add(new StringContent(uploaddto.ConversationId.ToString()), "conversationId");
            try
            {


                var result = await _httpClient.PostAsync("/api/v1/message/UploadFile",content, cancellationToken);
                if (result.IsSuccessStatusCode)
                {
                    var resultapi = await result.Content.ReadAsStringAsync();
                    var FileResult = JsonSerializer.Deserialize<BaseResponseDto<UploadFileResult>>(resultapi, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    return (true, "عملیات با موفقیت انجام شد. ", FileResult!.result);
                }
                return (false, "مشکلی در بارگذاری فایل شما به وجود آمده ",null);
            }
            catch (Exception e)
            {

                return (false, "مشکلی در بارگذاری فایل شما به وجود آمده  لطفا مدتی دیگر مجدد تلاش فرمایید!",null);
            }
        }
        public async Task<(bool, string message, UploadFileResult? result)> UploadChannelFileAsync(UploadFileDto uploaddto, CancellationToken cancellationToken = default)
        {
            var content = new MultipartFormDataContent();
            uploaddto.file.Headers.ContentType = new MediaTypeHeaderValue(uploaddto.ContentType);

            content.Add(uploaddto.file, "file", uploaddto.fileName);
            content.Add(new StringContent(uploaddto.caption ?? ""), "caption");
            content.Add(new StringContent(uploaddto.ConversationId.ToString()), "channelId");
            try
            {


                var result = await _httpClient.PostAsync("/api/v1/channel/UploadFile", content, cancellationToken);
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

        public async Task<Stream?> GetFileById(Guid Id, CancellationToken cancellationToken=default)
        {
            var result = await _httpClient.GetAsync($"api/v1/chatfiles/{Id}/files", cancellationToken);
            if (!result.IsSuccessStatusCode)
            {
                return null;
            }
            var stream = await result.Content.ReadAsStreamAsync();
            return stream;
        }
       public async Task<ChatPhotoMessageDto?> GetImageById(Guid Id, CancellationToken cancellationToken = default)
        {
            var result = await _httpClient.GetAsync($"api/v1/chatfiles/{Id}/files", cancellationToken);
            if (!result.IsSuccessStatusCode)
            {
                return null;
            }
            var response = await result.Content.ReadAsStringAsync();
            var photoData = JsonSerializer.Deserialize<ChatPhotoMessageDto>(response, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            return photoData;
        }

        public async Task<(bool issuccess, string Message)> newReactionOnMessage(string? reaction, Guid MessageId)
        {
            var result = await _httpClient.PostAsJsonAsync($"api/v1/message/newReactionOnMessage", new { Reaction = reaction, MessageId }, new System.Text.Json.JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
            });
            if (result.IsSuccessStatusCode)
            {
                return (true, "ok");
            }
            return (false, "ارسال ری اکشن با خطا مواجه شد!");

        }
    }
}
