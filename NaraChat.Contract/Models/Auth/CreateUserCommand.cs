using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace NaraChat.Contract.Models.Auth
{
    public record CreateUserCommand 
    {
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string UserName { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string? phoneNumber { get; set; }
        public int Age { get; set; }
        public int Gender { get; set; }
        public Address Address { get; set; } = null!;
        public string? Email { get; set; }
    }
}
