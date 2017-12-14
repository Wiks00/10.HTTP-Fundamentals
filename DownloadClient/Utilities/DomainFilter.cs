using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DownloadClient.Utilities
{
    [Flags]
    public enum DomainFilter : byte
    {
        Domain = 0,
        Url = 2,
        None = 3
    }
}
