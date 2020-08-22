using System.Collections;
using System.Drawing;
using System.Linq;
using System.Net;
using CommandDotNet;
using CommandDotNet.Builders;
using CommandDotNet.IoC.MicrosoftDependencyInjection;
using ConsoleTables;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenQA.Selenium.Remote;
using PuppeteerSharp;
using ScreenshoterTest.Services;

namespace ScreenshoterTest
{
    class Program
    {
        static int Main(string[] args)
        {
            var host = Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) =>
                    {
                        services.AddTransient<IFileOpeningService, FileOpeningService>();
                        services.AddTransient<IScreenshotTaker, ScreenshotTaker>();
                        services.AddTransient<IWebDriverFactory, WebDriverFactory>();
                        services.AddTransient<IScreenshotFormatter, ScreenshotFormatter>();
                        services.AddTransient<IScreenshotSaver, ScreenshotSaver>();
                        services.AddTransient<IApp, App>();

                    }).Build();

            return new AppRunner<IApp>().UseMicrosoftDependencyInjection(host.Services).Run(args);
        }

    }
}
