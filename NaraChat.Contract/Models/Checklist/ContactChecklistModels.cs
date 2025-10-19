using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaraChat.Contract.Models.Checklist
{
    public sealed class ContactChecklistData
    {
        public List<ContactChecklistItem> Items { get; set; } = new();
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    }
}
