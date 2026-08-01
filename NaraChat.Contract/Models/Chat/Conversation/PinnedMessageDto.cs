using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaraChat.Contract.Models.Chat.Conversation
{
    public sealed class PinnedMessageDto
    {
        public Guid Id { get; set; }
        public string? Content { get; set; }
        public string SenderName { get; set; } = string.Empty;
        public DateTime SendAt { get; set; }
        public DateTime? PinnedAt { get; set; }
        public int Type { get; set; }
        public string? FileName { get; set; }
    }
}
