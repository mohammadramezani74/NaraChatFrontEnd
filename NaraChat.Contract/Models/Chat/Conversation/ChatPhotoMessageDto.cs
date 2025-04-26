using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaraChat.Contract.Models.Chat.Conversation
{
    public sealed class ChatPhotoMessageDto
    {
        public string? Data { get; set; }
        public string? Thumbnail { get; set; }
        public string? fileDownloadName { get; set; }
    }
}
