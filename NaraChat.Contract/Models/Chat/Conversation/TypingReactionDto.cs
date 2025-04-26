using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaraChat.Contract.Models.Chat.Conversation
{
    public record TypingReactionDto(Guid UserId,Guid MyUserId, int MessageType);
}
