using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using DownloadClient;
using DownloadClient.EventArgs;

namespace ConsoleUI
{
    class Program
    {
        static void Main(string[] args)
        {
            var uri = new Uri("https://www.slideshare.net/peterbuck/download-presentation-in-powerpoint");

            var downloader = new Downloader(uri, DomainFilter.Domain, 1, ".js", ".css");

            downloader.ContentEvent += DownloaderOnContentEvent;
            downloader.SiteEvent += DownloaderOnSiteEvent;
        
            downloader.Start();

            Console.ReadKey();
        }

        private static void DownloaderOnSiteEvent(object sender, RaiseEventEventArgs raiseEventEventArgs)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(raiseEventEventArgs.EventStage);
            Console.WriteLine(raiseEventEventArgs.EventName);
            Console.WriteLine(raiseEventEventArgs.Message);

            Console.ResetColor();
            Console.WriteLine();
        }

        private static void DownloaderOnContentEvent(object sender, RaiseEventEventArgs raiseEventEventArgs)
        {
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine(raiseEventEventArgs.EventStage);
            Console.WriteLine(raiseEventEventArgs.EventName);
            Console.WriteLine(raiseEventEventArgs.Message);

            Console.ResetColor();
            Console.WriteLine();
        }
    }
}
