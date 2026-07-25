using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaraChat.Contract.Models.Chat.Group
{
    public class CreateGroupCommand
    {
      public List<Guid> Others { get; set; }
        public string Title { get; set; }
        public string UserName { get; set; }
        public string? Description { get; set; }
    }
}
