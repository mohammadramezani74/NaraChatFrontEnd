using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaraChat.Contract.Models.Auth
{
    public class Address
    {
      
            public Address(string city, string? street, string? postalCode)
            {
                City = city;
                Street = street;
                PostalCode = postalCode;
            }
            private Address()
            {

            }
            public string City { get; }
            public string? Street { get; }
            public string? PostalCode { get; }
        }
    
}
