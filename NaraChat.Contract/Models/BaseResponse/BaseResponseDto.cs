using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaraChat.Contract.Models.BaseResponse
{
    public class BaseResponseDto<T>
    {
            public int count { get; set; }
            public T result { get; set; }
            public int status { get; set; }
            public string message { get; set; }
            public bool isSucceded { get; set; }
        
    }
}
