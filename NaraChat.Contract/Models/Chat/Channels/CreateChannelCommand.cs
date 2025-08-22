using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaraChat.Contract.Models.Chat.Channels
{
    public sealed record CreateChannelCommand(string Title,
        string Description,
        string UserName,
        bool IsPublic
        );
}
