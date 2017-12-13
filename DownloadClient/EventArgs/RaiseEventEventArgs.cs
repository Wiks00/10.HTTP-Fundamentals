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

        public Event EventName { get; set; }

        public string[] EventArgs { get; set; }
    }

    public enum EventStage
    {
        Started = 0,
        Ended = 1,
        Terminated = 2,
    }

    public enum Event
    {
        File = 0,
        Site = 1
    }
}
