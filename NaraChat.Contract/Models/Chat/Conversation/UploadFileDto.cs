using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaraChat.Contract.Models.Chat.Conversation
{
    public sealed record UploadFileDto(Guid ConversationId, string? caption,
        StreamContent file, string ContentType, string extension,string fileName);

}
