using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using DownloadClient;

namespace ConsoleUI
{
    class Program
    {
        static void Main(string[] args)
        {
            var uri = new Uri("https://www.slideshare.net/peterbuck/download-presentation-in-powerpoint");

            var downloader = new DownloadClient.Downloader(uri, DomainFilter.Domain, 7);

            downloader.Start();

            Console.ReadKey();
        }
    }
}
