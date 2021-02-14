using CovidDataExtractor.DTO;
using HtmlAgilityPack;
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
        public WebScrapingService(HttpClient client)
        {
            this.client = client;
        }
        public async Task<ParsedBitmap> DownloadImage(string url)
        {
            using var response = client.GetAsync(url);
            Stream stream = await response.Result?.Content?.ReadAsStreamAsync();
            if (stream is null)
                return null;
            Bitmap bmp = new Bitmap(stream);

            return new ParsedBitmap()
            {
                Url = url,
                Image = bmp
            };
        }
        public async Task<List<string>> ParseHtml(string url)
        {
            string imageUrlStart = @"Health-Info-Site/PublishingImages/health-info/diseases-conditions/covid-19/case-counts-press-statements/covid19_lha_";
            HtmlDocument html = new HtmlDocument();
            string data = await CallUrl(url);
            html.LoadHtml(data);
            var urlList = html
                .DocumentNode
                .Descendants("a")
                .Where(x => x.GetAttributeValue("href", "").Contains(imageUrlStart))
                .ToList();
            List<string> list = new List<string>();
            foreach (var item in urlList)
            {
                string link = item
                    .GetAttributeValue("href", "")
                    .Replace("&#58;", ":");
                link = link.StartsWith("/Health-Info-Site") ? link.Insert(0, "http://www.bccdc.ca") : link;
                if (!link.Contains("cumulative"))
                    list.Add(link);
            }
            return list;
        }
        public DateRange ParseDatesFromUrl(ProcessedBitmap processed)
        {
            DateRange dates = new DateRange();
            if (processed.Location == CropLocation.Chilliwack)
            {
                string bcDateFormat = "yyyyMMdd";
                try
                {
                    CultureInfo provider = new CultureInfo("en-CA");
                    int firstIndex = processed.Url.IndexOf("lha_");
                    string from = processed.Url.Substring(firstIndex + 4, 8);
                    string to = processed.Url.Substring(firstIndex + 13, 8);
                    dates.FromDate = DateTime.ParseExact(from, bcDateFormat, provider);
                    dates.ToDate = DateTime.ParseExact(to, bcDateFormat, provider);
                }
                catch { }
            }
            return dates;
        }
        private async Task<string> CallUrl(string url)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls13;
            return await client.GetStringAsync(url);
        }
    }
}
