using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DownloadClient.Utilities;

namespace DownloadClient.EventArgs
{
    public class RaiseEventEventArgs
    {
        public EventStage EventStage { get; set; }

        public string EventName { get; set; }

        public string Message { get; set; }
    }



}
