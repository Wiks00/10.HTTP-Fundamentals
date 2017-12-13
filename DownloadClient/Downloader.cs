﻿using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using CsQuery;
using CsQuery.ExtensionMethods;
using DownloadClient.EventArgs;
using DownloadClient.Exceptions;
using DownloadClient.Utilities;

namespace DownloadClient
{
    public class Downloader : IDisposable
    {
        private readonly int maxLevel; 
        private readonly Uri startUri;
        private readonly string downloadPath = @"D:\Path\";
        private readonly ConcurrentHashSet<string> fileExtenthionFilter;
        private readonly DomainFilter filterFlag;
        private readonly HashSet<string> sites = new HashSet<string>();
        private readonly ConcurrentHashSet<string> content = new ConcurrentHashSet<string>();

        public Downloader(Uri uri, DomainFilter filterFlag, int maxLevel = 0, params string[] fileExtenthionFilter)
        {
            this.maxLevel = maxLevel;
            startUri = uri;
            this.filterFlag = filterFlag;
            this.fileExtenthionFilter = new ConcurrentHashSet<string>(fileExtenthionFilter);
        }

        public event EventHandler<RaiseEventEventArgs> ContentEvent;
        public event EventHandler<RaiseEventEventArgs> SiteEvent;

        protected virtual void OnContentEvent(RaiseEventEventArgs e)
            => ContentEvent?.Invoke(this, e);

        protected virtual void OnSiteEvent(RaiseEventEventArgs e)
            => SiteEvent?.Invoke(this, e);

        public void Start()
            => StartAsync().Wait();

        public async Task StartAsync()
            => await StartAsync(NewLevel(startUri), startUri.PathAndQuery, 0);

        private async Task StartAsync(HttpClient httpClient, string requestUri, int level)
        {
            OnSiteEvent(new RaiseEventEventArgs
            {
                EventStage = EventStage.Start,
                EventName = "Parsing",
                Message = $"{httpClient.BaseAddress + requestUri} on {level} level"
            });

            if (!sites.Add(httpClient.BaseAddress + requestUri))
            {
                OnSiteEvent(new RaiseEventEventArgs
                {
                    EventStage = EventStage.Terminate,
                    EventName = "Duplicated",
                    Message = $"{httpClient.BaseAddress + requestUri}"
                });

                return;
            }

            List<Task> downlodTasks = new List<Task>();

            try
            {
                Stream html = await httpClient.GetStreamAsync(requestUri);

                CQ cq = CQ.Create(html, Encoding.Default, HtmlParsingMode.Content,
                    HtmlParsingOptions.AllowSelfClosingTags | HtmlParsingOptions.IgnoreComments);

                var links = cq["[href]:not([href='']):not([href^='//']):not([href^='/'])"]
                    .Select(item => item.GetAttribute("href"));

                foreach (var link in links)
                {
                    if (link.StartsWith("http"))
                    {
                        var uri = new Uri(link);

                        if (uri.AbsolutePath.Contains('.'))
                        {
                            downlodTasks.Add(DownloadFile(uri.AbsoluteUri));
                        }
                        else
                        {
                            if (filterFlag == DomainFilter.Domain)
                            {
                                if (uri.Host != startUri.Host)
                                {
                                    OnSiteEvent(new RaiseEventEventArgs
                                    {
                                        EventStage = EventStage.Terminate,
                                        EventName = $"Skiped by {filterFlag} filter",
                                        Message = $"{uri.AbsoluteUri} on {level} level"
                                    });

                                    continue;
                                }
                            }

                            if (filterFlag == DomainFilter.Url)
                            {
                                if (uri.Host != startUri.Host || uri.Segments.Length > startUri.Segments.Length)
                                {
                                    OnSiteEvent(new RaiseEventEventArgs
                                    {
                                        EventStage = EventStage.Terminate,
                                        EventName = $"Skiped by {filterFlag} filter",
                                        Message = $"{uri.AbsoluteUri} on {level} level"
                                    });

                                    continue;
                                }
                            }

                            if (level < maxLevel)
                            {
                                await StartAsync(NewLevel(uri), uri.PathAndQuery, level + 1);
                            }
                        }
                    }

                    if (link.StartsWith("/"))
                    {
                        if (link.Contains('.'))
                        {
                            downlodTasks.Add(DownloadFile(httpClient.BaseAddress + link));
                        }
                        else
                        {
                            if (filterFlag == DomainFilter.Url)
                            {
                                if (link.Split('/').Length > startUri.Segments.Length)
                                {
                                    OnSiteEvent(new RaiseEventEventArgs
                                    {
                                        EventStage = EventStage.Terminate,
                                        EventName = $"Skiped by {filterFlag} filter",
                                        Message = $"{httpClient.BaseAddress + link} on {level} level"
                                    });

                                    continue;
                                }
                            }

                            if (level < maxLevel)
                            {
                                await StartAsync(httpClient, link, level + 1);
                            }
                        }
                    }
                }

                var contentLinks = cq["[src]:not([src='']):not(iframe):not([src^='//'])"]
                    .Select(item => item.GetAttribute("src"));

                foreach (var contentLink in contentLinks)
                {
                    var link = contentLink;

                    if (contentLink.StartsWith("/"))
                    {
                        link = httpClient.BaseAddress + contentLink;
                    }

                    downlodTasks.Add(DownloadFile(link));
                }

                await Task.WhenAll(downlodTasks);
            }
            catch (FileRequestException ex)
            {
                OnContentEvent(new RaiseEventEventArgs
                {
                    EventStage = EventStage.Terminate,
                    EventName = "Exception",
                    Message = ex.Message
                });
            }
            catch (HttpRequestException ex)
            {
                OnSiteEvent(new RaiseEventEventArgs
                {
                    EventStage = EventStage.Terminate,
                    EventName = "Exception",
                    Message = ex.Message
                });
            }
            finally
            {
                httpClient.Dispose();

                OnSiteEvent(new RaiseEventEventArgs
                {
                    EventStage = EventStage.End,
                    EventName = "Parsing",
                    Message = $"{httpClient.BaseAddress + requestUri} on {level} level"
                });
            }
        }

        private async Task DownloadFile(string fileUri)
        {
            //ToDo: ext filter

            OnContentEvent(new RaiseEventEventArgs
            {
                EventStage = EventStage.Start,
                EventName = "Downloading",
                Message = fileUri
            });

            string fileName;
            byte[] contentData;

            try
            {
                if (fileUri.StartsWith("data:image/"))
                {
                    fileName = Path.GetRandomFileName() + "." + fileUri.Split(';')[0].Split('/')[1];

                    contentData = Convert.FromBase64String(fileUri.Split(',')[1]);
                }
                else
                {
                    if (fileUri.Contains('?'))
                    {
                        fileUri = fileUri.Split('?')[0];
                    }

                    fileName = fileUri.Substring(fileUri.LastIndexOf('/') + 1);

                    using (var client = new HttpClient())
                    {
                        contentData = await client.GetByteArrayAsync(fileUri);
                    }
                }

                if (fileName.Contains('.'))
                {
                    var fileExt = '.' + fileName.Split('.')[1];

                    if (fileExtenthionFilter.Contains(fileExt))
                    {
                        OnContentEvent(new RaiseEventEventArgs
                        {
                            EventStage = EventStage.Terminate,
                            EventName = $"Skiped by filter: {string.Join("; ", fileExtenthionFilter)}",
                            Message = fileName
                        });

                        return;
                    }
                }

                if (content.Add(fileName))
                {
                    using (var file = new FileStream(downloadPath + fileName, FileMode.Create, FileAccess.Write))
                    {
                        await file.WriteAsync(contentData, 0, contentData.Length);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new FileRequestException(ex.Message, ex); 
            }

            OnContentEvent(new RaiseEventEventArgs
            {
                EventStage = EventStage.End,
                EventName = "Downloading",
                Message = downloadPath + fileName
            });
        }

        private HttpClient NewLevel(Uri uri)
            => new HttpClient
                    {
                        BaseAddress = new Uri(uri.AbsoluteUri.Replace(uri.AbsolutePath == "/" ? " " : uri.AbsolutePath, string.Empty).Split('?')[0])
                    };

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                content?.Dispose();
            }
        }

        ~Downloader()
        {
            Dispose(false);
        }
    }

    [Flags]
    public enum DomainFilter : byte
    {
        Domain = 0,
        Url = 2,
        None = 3
    }
}
