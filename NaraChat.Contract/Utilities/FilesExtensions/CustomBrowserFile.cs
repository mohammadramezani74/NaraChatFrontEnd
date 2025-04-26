using Microsoft.AspNetCore.Components.Forms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaraChat.Contract.Utilities.FilesExtensions
{
    public class CustomBrowserFile : IBrowserFile
    {
        private readonly byte[] _data;
        private readonly string _name;
        private readonly string _contentType;

        public CustomBrowserFile(byte[] data, string name, string contentType)
        {
            _data = data;
            _name = name;
            _contentType = contentType;
        }

        public string Name => _name;
        public DateTimeOffset LastModified => DateTimeOffset.Now;
        public long Size => _data.Length;
        public string ContentType => _contentType;

        public Stream OpenReadStream(long maxAllowedSize = 512000, CancellationToken cancellationToken = default)
        {
            return new MemoryStream(_data);
        }
    }
}
