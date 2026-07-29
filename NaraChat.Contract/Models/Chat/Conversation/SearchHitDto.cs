using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaraChat.Contract.Models.Chat.Conversation
{
    public sealed class SearchHitDto
    {
        public Guid Id { get; set; }
        public DateTime SendAt { get; set; }
        public string? Content { get; set; }
        public string SenderName { get; set; } = string.Empty;
        public Guid SenderId { get; set; }
        public bool IsMine { get; set; }
        public int Type { get; set; }
        public string? FileName { get; set; }
    }

    public sealed class SearchPageDto
    {
        public List<SearchHitDto> Items { get; set; } = new();
        public DateTime? NextCursor { get; set; }
        public bool HasMore { get; set; }
    }
}
