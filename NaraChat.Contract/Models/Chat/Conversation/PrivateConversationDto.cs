using NaraChat.Contract.Models.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaraChat.Contract.Models.Chat.Conversation;



    public class PrivateConversationDto
{
        public Guid id { get; set; }
        public string chatType { get; set; }
    public string title { get; set; } = "لطفا کاربری را برای چت انتخاب نمایید!";
    public List<UserDto> users { get; set; } = new();
    }




