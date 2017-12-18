using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using DownloadClient;
using DownloadClient.EventArgs;
using DownloadClient.Utilities;

namespace ConsoleUI
{
    class Program
    {
        static void Main(string[] args)
        {
            //~ 10 min
            var uri = new Uri("https://www.bsuir.by/ru/magistratura");

            var downloader = new Downloader(uri, "D:\\Path", 1, ".js", ".css", ".html", ".php");

            downloader.ContentEvent += DownloaderOnEvent;
            downloader.SiteEvent += DownloaderOnEvent;
        
            downloader.Start();

            Console.ReadKey();
        }

        private static void DownloaderOnEvent(object sender, RaiseEventEventArgs raiseEventEventArgs)
        {
            Console.WriteLine(raiseEventEventArgs.EventStage);
            Console.WriteLine(raiseEventEventArgs.EventName);
            Console.WriteLine(raiseEventEventArgs.Message);

            Console.WriteLine();
        }
    }
}
