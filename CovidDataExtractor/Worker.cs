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
        private readonly ILogger<Worker> _logger;
        private readonly IWebScrapingService webService;
        private readonly IOcrService ocrService;
        private readonly IRepository repo;

        public Worker(ILogger<Worker> logger, 
            IWebScrapingService service, 
            IOcrService ocrService,
            IRepository repo
            )
        {
            _logger = logger;
            this.webService = service;
            this.ocrService = ocrService;
            this.repo = repo;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {

            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                List<Data> dataList = new List<Data>();
               List<string> list = await webService.ParseHtml(@"http://www.bccdc.ca/health-info/diseases-conditions/covid-19/data");
                list.ForEach(async x =>
                {
                    ParsedBitmap parsed = await webService.DownloadImage(x);
                    ProcessedBitmap processed = ocrService.Extract(parsed, CropLocation.Chilliwack);
                    DateRange dates = webService.ParseDatesFromUrl(processed);
                    dataList.Add(Data.Convert(processed, dates));
                });
                dataList.ForEach(async x => await  repo.Add(x));

                await Task.Delay(60000, stoppingToken);

            }
        }

    }
}
