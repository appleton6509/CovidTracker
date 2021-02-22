using CovidDataExtractor.DTO;
using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace CovidDataExtractor.Services
{
    public class WebScrapingService : IWebScrapingService
    {
        private HttpClient client;
        private ILogger<WebScrapingService> log;
        public WebScrapingService(HttpClient client, ILogger<WebScrapingService> log)
        {
            this.client = client;
            this.log = log;
        }
        public async Task<ParsedBitmap> DownloadImage(string url)
        {
            Bitmap bmp = null;
            try
            {
                using var response = client.GetAsync(url);
                Stream stream = await response.Result?.Content?.ReadAsStreamAsync();
                 bmp = new Bitmap(stream);
            } catch (Exception e)
            {
                log.LogError("Unable to download image: " + e.Message);
            }
            return new ParsedBitmap()
            {
                Url = url,
                Image = bmp
            };
        }
        public async Task<List<string>> ParseHtml(string url)
        {
            List<string> list = new List<string>();
            string imageUrlStart = @"Health-Info-Site/PublishingImages/health-info/diseases-conditions/covid-19/case-counts-press-statements/covid19_lha_";
            HtmlDocument html = new HtmlDocument();
            try
            {
                string data = await CallUrl(url);
                html.LoadHtml(data);
                var urlList = html
                    .DocumentNode
                    .Descendants("a")
                    .Where(x => x.GetAttributeValue("href", "").Contains(imageUrlStart))
                    .ToList();

                foreach (var item in urlList)
                {
                    string link = item
                        .GetAttributeValue("href", "")
                        .Replace("&#58;", ":");
                    link = link.StartsWith("/Health-Info-Site") ? link.Insert(0, "http://www.bccdc.ca") : link;
                    if (!link.Contains("cumulative"))
                        list.Add(link);
                }
            } catch (Exception e)
            {
                log.LogError("Error parsing html: " + e.Message);
            }

            return list;
        }
        public DateRange ParseDateRangeFromUrl(ProcessedBitmap processed)
        {
            return ParseDateRangeFromUrl(processed.Url, processed.Location);
        }
        public DateRange ParseDateRangeFromUrl(string url, Location local)
        {
            DateRange dateRange = new DateRange();
            if (local == Location.Chilliwack)
            {
                string bcDateFormat = "yyyyMMdd";
                try
                {
                    CultureInfo provider = new CultureInfo("en-CA");
                    int firstIndex = url.IndexOf("lha_");
                    string from = url.Substring(firstIndex + 4, 8);
                    string to = url.Substring(firstIndex + 13, 8);
                    dateRange.FromDate = DateTime.ParseExact(from, bcDateFormat, provider);
                    dateRange.ToDate = DateTime.ParseExact(to, bcDateFormat, provider);
                }
                catch (Exception e)
                {
                    log.LogWarning("Parse Date Failed: " + e.Message);
                    dateRange = null;
                }
            }
            return dateRange;
        }

        private async Task<string> CallUrl(string url)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls13;
            return await client.GetStringAsync(url);
        }
    }
}
