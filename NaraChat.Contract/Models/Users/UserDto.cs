using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaraChat.Contract.Models.Users
{
    public partial class UserDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public DateTime? LastSeen { get; set; }
        public string? avatar { get; set; }
        public bool IsOnline { get; set; }
        public bool IsSelected { get; set; }
        public int messageUnreadedCount { get; set; }
        public string? LastReceivedMessage { get; set; }
        public Guid? LastReceivedMessageId { get; set; }
        public bool IsLastReceivedMessageForMe { get; set; }

        public UserDto(Guid id, string name, string? Avatar = null, bool Isonline = false, int messageUnreadedCount = 0, DateTime? lastseen = null, string? lastReceivedMessage = null,
            Guid? lastReceivedMessageId=null,bool isLastReceivedMessageForMe=false)
        {
            Id = id;
            Name = name;
            avatar = Avatar;
            IsOnline = Isonline;
            this.messageUnreadedCount = messageUnreadedCount;
            LastSeen = lastseen;
            LastReceivedMessage = lastReceivedMessage;
            LastReceivedMessageId = lastReceivedMessageId;
            IsLastReceivedMessageForMe=isLastReceivedMessageForMe;
        }
    }
    public class UserAvatar
    {
        public string? MyAvatar { get; set; }
        public string? OtherAvatar { get; set; }

    }
}
