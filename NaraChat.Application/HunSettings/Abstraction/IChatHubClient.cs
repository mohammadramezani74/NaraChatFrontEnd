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

        /// <summary>آخرین پیام گفتگو عوض شد؛ لیست مکالمات باید به‌روز شود.</summary>
        Task LastMessageChanged(LastMessageChangedDto Message);
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
        Task MessagePinChanged(Guid scopeId, Guid messageId, bool pinned);

        /// <summary>کارهای شخصی کاربر درباره‌ی یک مخاطب از دستگاه یا تب دیگری عوض شد.</summary>
        Task ContactTasksChanged(Guid contactId);

        /// <summary>زمان یادآوری یکی از کارهای کاربر رسید.</summary>
        Task TaskReminder(NaraChat.Contract.Models.ContactTasks.TaskReminderDto reminder);

        /// <summary>ربات یکی از پیام‌های خودش را عوض کرد (متن یا دکمه‌ها).</summary>
        Task BotMessageUpdated(ChatMessageDto message);

        /// <summary>کیبورد ربات برای این کاربر عوض شد.</summary>
        Task BotStateChanged(NaraChat.Contract.Models.Bots.BotStateDto state);

    }
}
