using NaraChat.Contract.Models.Chat.Conversation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaraChat.Application.Services.ChatServices.Conversation
{
    public interface IMessageService
    {
         Task<List<ChatMessageDto>?>LoadMessages(Guid ConverSationId,int MessageCount=50,CancellationToken cancellationToken=default);
        Task<(bool, Guid MessageId)> SendMessageAsync(Guid ConversationId, string Message,Guid? ParentId=null, float? latitude = null, float? Longitude = null, CancellationToken cancellationToken = default);
        Task<(bool, string message)> EditMessageAsync(EditedMessageDto messageDto, CancellationToken cancellationToken = default);
        Task<(bool, string message)> DeleteMessageAsync(Guid MessageId,Guid OtherId, CancellationToken cancellationToken = default);
        Task<(bool, string message, UploadFileResult? result)>UploadChatFileAsync(UploadFileDto uploaddto, CancellationToken cancellationToken = default);
        Task<Stream?> GetFileById(Guid Id,CancellationToken cancellationToken=default);
        Task<ChatPhotoMessageDto?> GetImageById(Guid Id, CancellationToken cancellationToken = default);
        Task<(bool issuccess,string Message)> newReactionOnMessage(string? reaction, Guid Message);
    }
}
