using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaraChat.Contract.Models.Auth
{
    public sealed record   TokenDto(string token,string refreshtoken);

}
