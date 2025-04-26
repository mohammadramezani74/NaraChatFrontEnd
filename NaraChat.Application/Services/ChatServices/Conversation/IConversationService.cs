using NaraChat.Contract.Models.Chat.Conversation;
using NaraChat.Contract.Models.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaraChat.Application.Services.ChatServices.Conversation
{
    public interface  IConversationService
    {
        Task<PrivateConversationDto?> ReadyOrCreateConversationBy(Guid ToUserId,CancellationToken cancellationToken=default(CancellationToken));
        Task<UserAvatar> GetImageConversation(Guid MyUserId,Guid? OtherUserId, CancellationToken cancellationToken = default(CancellationToken));
    }
}
