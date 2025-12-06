using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaraChat.Contract.Models.Chat.Channels
{
    public class ChannelFileItem
    {
        public Guid Id { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string FileData { get; set; } = string.Empty;
    }
}
