using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaraChat.Contract.Models.Chat.Channels
{
    public class ChannelDto
    {
        public string Creator { get; set; }
        public Guid CreatorId { get; set; }
        public List<UserChannelDto> admins { get; set; } = new();
        public bool CurrentUserAdmin { get; set; }
    }
}
