using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;

namespace NaraChat.Contract.Models.Users.ServiceViewModels
{
    public sealed record GetUsersViewModel
    {
        public Guid Id { get; set; }
        public string UserName { get; set; } = null!;
        public DateTime? LastSeen { get; set; }
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string Avatar { get; set; }
        public int MessageUnreadedCount { get; set; }
        public string LastReceivedMessage { get; set; }
        public Guid? LastReceivedMessageId { get; set; }
        public bool IsLastReceivedMessageForMe { get; set; }
    }
}
