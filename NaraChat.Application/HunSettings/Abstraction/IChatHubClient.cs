using NaraChat.Contract.Models.Chat.Conversation;
using NaraChat.Contract.Models.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaraChat.Application.HunSettings.Abstraction
{
    public interface IChatHubClient
    {
        Task UserConnected(UserDto user);
        Task OnlineUserList(IEnumerable<UserDto> users);
        Task UserIsOnline(Guid UserId);
        Task UserIsOffline(Guid UserId);
        Task MessagedReceived(ChatMessageDto message);
        Task EditedMessageReceived(EditedMessageDto message);
        Task DeletedMessageReceived(Guid MessageId);
        Task MessagedSeenReceived(List<Guid> MessageId);
        Task IncreaseMessageCount(Guid UserId);
        Task ReceivedNotifications(NotificationModelDto notify);
        Task SetLastSeenUser(LastSeenModelDto lastSeenModel);
        Task ReceivedReactions(TypingReactionDto MessageType);
        Task ReceivedEmojiReact(MessageReaction reaction);
        Task GetMissedMessages(List<ChatMessageDto> messages);
        Task BlockUser(BlockDto blockDto);
        Task GetDeletedChannel(Guid channelId);
        Task ReceiveNewChannel(UserDto user);
        Task ChatHistoryCleared(Guid conversationOrChannelId);
    }
}
