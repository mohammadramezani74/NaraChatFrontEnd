using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaraChat.Contract.Models.Users
{
    public sealed class TargetUserInfoResponse
    {
      public Guid Id { get; set; }
       public string Name { get; set; }
        public string Avatar { get; set; }
        public string bio { get; set; }
        public string Address { get; set; }
        public string phoneNumber { get; set; }
        public string Email { get; set; }
    }

        
}
