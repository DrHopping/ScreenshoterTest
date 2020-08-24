using CommandDotNet;
using CommandDotNet.IoC.MicrosoftDependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
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
                        services.AddTransient<IConsoleWriter, ConsoleWriter>();
                        services.AddTransient<IResultSaver, ResultSaver>();
                        services.AddTransient<IApp, App>();
                    }).Build();

            return new AppRunner<IApp>().UseMicrosoftDependencyInjection(host.Services).Run(args);
        }

    }
}
