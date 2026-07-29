using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaraChat.Contract.Models.Chat.Conversation
{
    public sealed class MessagesPageDto
    {
        public List<ChatMessageDto> Items { get; set; } = new();

      
        public DateTime? NextCursor { get; set; }

        public bool HasMore { get; set; }
    }
}
