using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace AsynchronousDownloads
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // url list
            List<Uri> urlList = new List<Uri>();
            urlList.Add(new Uri("https://doubleyolk.co/"));
            urlList.Add(new Uri("https://doubleyolk.co/case-studies/"));
            urlList.Add(new Uri("https://doubleyolk.co/blog"));

            //CancellationTokenSource to handle if call is cancelled 
            using (var cancellationTokenSource = new CancellationTokenSource())
            {
                var contentLength = await GetAggregatedContentLength(urlList, cancellationTokenSource.Token);
                Console.WriteLine($"Aggregated content length of all 3 responses is {contentLength}");
            }

        }

        private static async Task<long> GetAggregatedContentLength(List<Uri> urlList, CancellationToken token)
        {
            //aggregatedResult to store objects
            var aggregatedResult = new ConcurrentBag<long>();
            await Task.Run(() => {
                Parallel.ForEach(urlList, (url) =>
                {
                    //webClient for sending and receiving data
                    using (var webClient = new WebClient())
                    {
                        //if token is cancelled it will cancell the request
                        token.Register(() =>
                        {
                            //cancells pending async operation
                            webClient.CancelAsync();
                            Console.WriteLine("Download request cancelled !");
                        });

                        //adding object to aggregatedResult
                        aggregatedResult.Add(webClient.DownloadData(url).Length);
                    }
                });
            });
            // calculating the aggregate content length of all 3 responses
            return aggregatedResult.Sum();
        }
    }
}
