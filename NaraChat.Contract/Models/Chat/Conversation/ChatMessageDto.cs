using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaraChat.Contract.Models.Chat.Conversation
{
    public sealed class ChatMessageDto
    {
        public Guid Id { get; set; }

        /// <summary>
        /// شناسه‌ی گفتگویی که پیام به آن تعلق دارد: ConversationId برای چت خصوصی
        /// و گروه، ChannelId برای کانال. برای تشخیص اینکه پیام مال گفتگوی باز است
        /// یا نه از همین استفاده کن، نه از UserId — معنای UserId بین مسیرها فرق
        /// می‌کند و همین باعث نشان داده شدن پیام یک گروه در گروه دیگر شده بود.
        /// </summary>
        public Guid ScopeId { get; set; }

        public Guid UserId { get; set; }

        /// <summary>
        /// نام فرستنده‌ی اصلی. اگر پیام فوروارد نباشد null است، پس همین برای
        /// تصمیم به نمایش برچسب کافی است.
        /// </summary>
        public string? ForwardedFromName { get; set; }
        public string? SenderName { get; set; }
        public string Content { get; set; } = string.Empty;
        public bool IsMute { get; set; }
        public bool IsMine { get; set; }
        public bool IsSeen { get; set; }
        public bool isEdited { get; set; }
        public Guid? ParentId { get; set; }
        public DateTime SendAt { get; set; }
        public MessageType Type { get; set; } = MessageType.Text;
        public ChatFilesDto? FileContent { get; set; }
        public string? Reaction { get; set; }
        public string? ParentContent { get; set; }
        public string? ParentSenderName { get; set; }
        public bool IsPinned { get; set; }
        public float? Latitude { get; set; }
        public float? Longitude { get; set; }
        public ConversationType ConversationType { get; set; }
    } public enum ConversationType
        {
            Private = 1,
            Channel = 2,
            group = 3
        }
}
