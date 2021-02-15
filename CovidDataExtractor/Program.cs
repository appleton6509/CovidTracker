using CovidDataExtractor.Entity;
using CovidDataExtractor.Repositories;
using CovidDataExtractor.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CovidDataExtractor
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddHostedService<Worker>();
                    var DevSettings = new ConfigurationBuilder()
                                                     .SetBasePath(hostContext.HostingEnvironment.ContentRootPath)
                                                     .AddJsonFile("appsettings.Development.json")
                                                     .Build();
                    services.AddDbContextFactory<CovidContext>((x) =>
                    {
                        if (hostContext.HostingEnvironment.IsDevelopment())
                            x.UseSqlServer(DevSettings.GetSection("ConnectionStrings")["Database"]);
                        else
                            x.UseSqlServer(hostContext.Configuration.GetSection("ConnectionStrings")["Database"]);
                    });


                    services.AddHttpClient();
                    services.AddTransient<IRepository, Repository>();
                    services.AddSingleton<IWebScrapingService, WebScrapingService>();
                    services.AddSingleton<IOcrService, OcrService>();
                    services.AddSingleton<IImagePreprocessor, ImagePreprocessor>();

                });
    }
}
