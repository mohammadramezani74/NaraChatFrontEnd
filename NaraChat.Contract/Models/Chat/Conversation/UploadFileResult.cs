using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaraChat.Contract.Models.Chat.Conversation
{
    public class UploadFileResult
    {
        public Guid FileId { get; set; }
        public Guid MessageId { get; set; }
        public int MessageType { get; set; }
    }
}
