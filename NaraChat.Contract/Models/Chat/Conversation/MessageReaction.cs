using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaraChat.Contract.Models.Chat.Conversation
{
    public sealed record MessageReaction(Guid MessageId,string? Reaction,Guid OtherUserId);
}
