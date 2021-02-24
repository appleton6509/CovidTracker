using CovidDataExtractor.DTO;
using CovidDataExtractor.Entity;
using CovidDataExtractor.Repositories;
using CovidDataExtractor.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tesseract;

namespace CovidDataExtractor
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> log;
        private readonly IWebScrapingService webService;
        private readonly IOcrService ocrService;
        private readonly IRepository repo;
        private static readonly Dictionary<Location, Rectangle> map = new Dictionary<Location, Rectangle>()
        {
              {Location.Chilliwack, new Rectangle(2290, 2098, 70,35) }
        };
        public Worker(ILogger<Worker> logger,
            IWebScrapingService service,
            IOcrService ocrService,
            IRepository repo
            )
        {
            log = logger;
            this.webService = service;
            this.ocrService = ocrService;
            this.repo = repo;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            log.LogInformation("Worker started at: {time}", DateTimeOffset.Now);
            while (!stoppingToken.IsCancellationRequested)
            {
                log.LogInformation("Worker running at: {time}", DateTimeOffset.Now);

                List<string> urls = await webService.ParseHtml(@"http://www.bccdc.ca/health-info/diseases-conditions/covid-19/data");
                List<Data> linkData = ProcessImagesFromUrls(urls);
                AddProcessedDataToDb(linkData);

                await Task.Delay(60000, stoppingToken);// 60 seconds
            }
        }

        private List<Data> ProcessImagesFromUrls(List<string> urls)
        {
            List<Data> linksData = new List<Data>();
            urls.ForEach(async url =>
            {
                try
                {
                    DateRange dateRange = webService.ParseDateRangeFromUrl(url, Location.Chilliwack);
                    bool dateExists = repo.Exists(dateRange.FromDate);
                    if (dateExists)
                        linksData = await ProcessSingleImageFromUrl(url, dateRange);
                }
                catch (Exception e)
                {
                    log.LogError("Error accessing DB: " + e.Message);
                }
            });
            return linksData;
        }

        private void AddProcessedDataToDb(List<Data> linksData)
        {
            linksData.ForEach(async x =>
            {
                try
                {
                    await repo.Add(x);
                }
                catch (Exception e)
                {
                    log.LogError($"Cannot add data from date: {x.ToDate} due to database error: " + e.Message);
                }
            });
        }

        private async Task<List<Data>> ProcessSingleImageFromUrl(string url, DateRange dateRange)
        {
            List<Data> linksData = new List<Data>();
            if (url is null || dateRange is null)
                return linksData;

            ParsedBitmap parsed = await webService.DownloadImage(url);
            ProcessedBitmap processed = ocrService.GetNumberFromProcessedImage(parsed, Location.Chilliwack, map[Location.Chilliwack]);
            linksData.Add(Data.Convert(processed, dateRange));
            return linksData;
        }
    }
}
