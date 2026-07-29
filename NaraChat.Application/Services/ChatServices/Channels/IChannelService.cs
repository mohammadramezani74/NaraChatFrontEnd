using NaraChat.Contract.Models.Chat.Channels;
using NaraChat.Contract.Models.Chat.Conversation;
using NaraChat.Contract.Models.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaraChat.Application.Services.ChatServices.Channels
{
    public  interface IChannelService
    {
        Task<(bool issuccess, string message)> CreateChannel(CreateChannelCommand command,CancellationToken cancellationToken=default);
        Task<(bool issuccess, string message)> ChangeDescription(ChangeBioChannelCommand command,CancellationToken cancellationToken=default);
        Task<(bool issuccess, string message)> PromoteToAdminOrDemoteUser(ChangeUserChannelPolicyCommand command, CancellationToken cancellationToken = default);
        Task<(bool issuccess, string message)> JoinToPublicChannel(Guid ChannelId, CancellationToken cancellationToken = default);

        Task<List<ChatMessageDto>?> LoadChannelMessages(Guid channelId, int Messagecount = 50, CancellationToken cancellationToken = default);
        Task<(bool, Guid MessageId)> SendMessageForChannelAsync(Guid ChannelId, string Message, Guid? ParentId = null, float? latitude = null, float? Longitude = null, CancellationToken cancellationToken = default);
        Task<List<UserDto>> GetPublicChannels(CancellationToken cancellationToken = default(CancellationToken));
        Task<(bool status,string message)> AddNewMemberByAdmin(AddNewMemberCommand command, CancellationToken cancellationToken = default);
        Task<List<ChannelMemberViewModel>> GetChannelMembers(Guid channelId, CancellationToken cancellationToken = default);
        Task<(bool success, string Message)> UploadNewProfileImage(StreamContent stream, string ContentType, string extension, Guid ChannelId, CancellationToken cancellationToken = default);
        Task<string?> GetAvatar(Guid UserId, CancellationToken cancellationToken = default);
        Task<string?> downloadChannelFile(Guid fileid, CancellationToken cancellationToken = default);
        Task<List<ChannelFileItem>?> getChannelFiles(Guid channelId, CancellationToken cancellationToken = default);
        Task<(bool status, string message)> RemoveMemberFromChannel(Guid memberid, Guid channelid, CancellationToken cancellationToken = default);
        Task<(bool status, string message)> ClearChannelHistory(
        Guid channelId, CancellationToken cancellationToken = default);

        Task<(bool status, string message)> DeleteChannel(
            Guid channelId, CancellationToken cancellationToken = default);
    }
}
