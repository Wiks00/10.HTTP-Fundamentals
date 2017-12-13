using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace DownloadClient.Exceptions
{
    [Serializable]
    public class FileRequestException : HttpRequestException
    {
        public FileRequestException() : base()
        {           
        }

        public FileRequestException(string message) : base(message)
        {
        }

        public FileRequestException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
