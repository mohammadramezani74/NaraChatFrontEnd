using NaraChat.Contract.Models.Chat.Channels;
using NaraChat.Contract.Models.Chat.Conversation;
using NaraChat.Contract.Models.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaraChat.Application.Services.ChatServices.Conversation
{
    public interface  IConversationService
    {
        Task<PrivateConversationDto?> ReadyOrCreateConversationBy(Guid ToUserId,CancellationToken cancellationToken=default(CancellationToken));
        Task<UserAvatar> GetImageConversation(Guid MyUserId,Guid? OtherUserId, CancellationToken cancellationToken = default(CancellationToken));
        Task<(bool IsSuccess, string Message)> PinConversation(bool IsPin, Guid ConversationId,CancellationToken cancellationToken=default(CancellationToken));
        Task<(bool IsSuccess, string Message)> BlockedConversation(bool Blocked, Guid ConversationId,CancellationToken cancellationToken=default(CancellationToken));
         Task<(bool IsSuccess, string Message)> CreateGroupConversation(List<Guid> Others, string Title, string UserName, string? Description, CancellationToken cancellationToken = default(CancellationToken));
        Task<List<ChannelMemberViewModel>> GetGroupMembers(Guid channelId, CancellationToken cancellationToken = default);
        Task<(bool status, string message)> RemoveMemberFromGroup(Guid ConversationId, Guid memberId, CancellationToken cancellationToken = default);
        Task<(bool issuccess, string message)> PromoteToAdminOrDemoteUser(ChangeUserGroupPolicyCommand command, CancellationToken cancellationToken = default);
        Task<(bool status, string message)> AddNewMemberByAdmin(AddNewMemberCommand command, CancellationToken cancellationToken = default);
        Task<List<ChatMessageDto>?> LoadGroupMessages(Guid conversationid, int Messagecount = 50, CancellationToken cancellationToken = default);
        Task<(bool, Guid MessageId, string message)> SendMessageForGroupAsync(Guid ConversationId, string Message, Guid? ParentId = null, float? latitude = null, float? Longitude = null, CancellationToken cancellationToken = default);
        Task<(bool status, string message)> MuteOrUnMuteUser(Guid ConversationId, Guid UserId,bool Mute ,CancellationToken cancellationToken = default);
        Task<(bool, string message, UploadFileResult? result)> UploadGroupFileAsync(UploadFileDto uploaddto, CancellationToken cancellationToken = default);
        Task<(bool success, string Message)> UploadGroupAvatarAsync(StreamContent stream, string ContentType, string extension, Guid ChannelId, CancellationToken cancellationToken = default);

        Task<List<ChannelFileItem>?> getGroupFiles(Guid groupId, CancellationToken cancellationToken = default);
        Task<string?> downloadGroupFile(Guid fileid, CancellationToken cancellationToken = default);
        Task<(bool issuccess, string message)> Changebio(ChangeBioGroupCommand command, CancellationToken cancellationToken = default);
        Task<(bool status, string message)> ClearConversationHistory(
    Guid conversationId, CancellationToken cancellationToken = default);

        Task<(bool status, string message)> ClearGroupHistory(
            Guid groupId, CancellationToken cancellationToken = default);

        Task<(bool status, string message)> DeleteGroup(
            Guid groupId, CancellationToken cancellationToken = default);
    }
}
