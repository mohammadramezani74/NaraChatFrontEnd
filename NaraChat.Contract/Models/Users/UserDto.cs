using NaraChat.Contract.Models.Chat.Channels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace NaraChat.Contract.Models.Users
{
    public partial class UserDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string? UserName { get; set; }
        public DateTime? LastSeen { get; set; }
        public string? avatar { get; set; }
        public bool IsOnline { get; set; }
        public bool IsSelected { get; set; }
        public int messageUnreadedCount { get; set; }
        public string? LastReceivedMessage { get; set; }
        public Guid? LastReceivedMessageId { get; set; }
        public bool IsLastReceivedMessageForMe { get; set; }
        public string? LastReceivedMessageSendDate { get; set; }
        public DateTime? LastMessageDate { get; set; }
        public bool IsPin { get; set; }
        public bool IsBlocked { get; set; }
        public bool OtherUserBlocked { get; set; }
        public Guid? ConversationId { get; set; }
        public bool IsChannel { get; set; }
        public ChannelDto? channel { get; set; }
        public UserDto()
        {
                
        }

        public UserDto(Guid id, string name, string? Avatar = null, bool Isonline = false, int messageUnreadedCount = 0, DateTime? lastseen = null, string? lastReceivedMessage = null,
            Guid? lastReceivedMessageId=null,bool isLastReceivedMessageForMe=false,string? lastReceivedMessageSendDate=null,bool isPinned=false,bool isBlocked=false,
        Guid? conversationId=null, bool otherUserBlocked = false, bool ischannel=false, ChannelDto?chanel=null,string? username=null,DateTime?lastmessageDate=null)
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
            LastReceivedMessageSendDate = lastReceivedMessageSendDate;
            IsPin = isPinned;
            IsBlocked = isBlocked;
            ConversationId = conversationId;
            OtherUserBlocked = otherUserBlocked;
            IsChannel=ischannel;
            channel= chanel;
            UserName = username;
            LastMessageDate = lastmessageDate;


        }
    }
    public class UserAvatar
    {
        public string? MyAvatar { get; set; }
        public string? OtherAvatar { get; set; }

    }
}
