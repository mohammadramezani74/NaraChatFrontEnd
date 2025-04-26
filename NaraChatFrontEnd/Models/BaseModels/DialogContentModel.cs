using MudBlazor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaraChat.Contract.Models.BaseResponse
{
    public  class DialogContentModel
    {
        public string? ContentText { get; set; }
        public string? ButtonText { get; set; }
        public Color Color { get; set; }
    }
}
