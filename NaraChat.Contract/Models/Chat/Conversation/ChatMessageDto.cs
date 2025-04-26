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
        public Guid UserId { get; set; }
        public string? SenderName { get; set; }
        public string Content { get; set; } = string.Empty;
        public bool IsMine { get; set; }
        public bool IsSeen { get; set; }
        public bool isEdited { get; set; }
        public Guid? ParentId { get; set; }
        public DateTime SendAt { get; set; }
        public MessageType Type { get; set; } = MessageType.Text;
        public ChatFilesDto? FileContent { get; set; }
        public string? Reaction { get; set; }
    }
}
