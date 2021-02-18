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
                List<Data> linksData = new List<Data>();
               List<string> urls = await webService.ParseHtml(@"http://www.bccdc.ca/health-info/diseases-conditions/covid-19/data");
                urls.ForEach(async x =>
                {
                    bool dateExists = true;
                    DateRange dateRange = null;
                    try
                    {
                        dateRange = webService.ParseDateRangeFromUrl(x, CropLocation.Chilliwack);
                        dateExists = repo.Exists(dateRange.FromDate);
                    } catch (Exception e)
                    {
                        log.LogError("Error accessing DB: " + e.Message);
                    }
                    if (!dateExists)
                    {
                        ParsedBitmap parsed = await webService.DownloadImage(x);
                        ProcessedBitmap processed = ocrService.ExtractText(parsed, CropLocation.Chilliwack);
                        linksData.Add(Data.Convert(processed, dateRange));
                    }
                });
                    linksData.ForEach(async x => {
                        try
                        {
                            await repo.Add(x);
                        }
                        catch (Exception e)
                        {
                            log.LogError($"Cannot add data from date: {x.ToDate} due to database error: " + e.Message);
                        }
                    });
                await Task.Delay(60000, stoppingToken);// 60 seconds
            }
        }

    }
}
