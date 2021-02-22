using CovidDataExtractor.DTO;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;

namespace CovidDataExtractor.Services
{
    public interface IWebScrapingService
    {
        Task<ParsedBitmap> DownloadImage(string url);
        Task<List<string>> ParseHtml(string url);
         DateRange ParseDateRangeFromUrl(ProcessedBitmap processed);
         DateRange ParseDateRangeFromUrl(string url, Location local);
    }
}