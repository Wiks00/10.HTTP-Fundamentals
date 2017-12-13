using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DownloadClient.EventArgs
{
    public class RaiseEventEventArgs
    {
        public EventStage EventStage { get; set; }

        public string EventName { get; set; }

        public string Message { get; set; }
    }

    public enum EventStage
    {
        Start = 0,
        End = 1,
        Terminate = 2
    }

}
