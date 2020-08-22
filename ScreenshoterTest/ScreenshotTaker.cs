using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using OpenQA.Selenium;
using ScreenshoterTest.Services;

namespace ScreenshoterTest
{
    public interface IScreenshotTaker
    {
        void TakeScreenshotsFromStream();
        void TakeScreenshotsFromFile(string inputPath, int timeout, int width, int height, string savePath, int threads);

    }
    public class ScreenshotTaker : IScreenshotTaker
    {
        private readonly IFileOpeningService _fileOpeningService;
        private readonly IWebDriverFactory _webDriverFactory;
        private readonly IScreenshotFormatter _screenshotFormatter;
        private readonly IScreenshotSaver _screenshotSaver;

        public ScreenshotTaker(IFileOpeningService fileOpeningService, IWebDriverFactory webDriverFactory, IScreenshotFormatter screenshotFormatter, IScreenshotSaver screenshotSaver)
        {
            _fileOpeningService = fileOpeningService;
            _webDriverFactory = webDriverFactory;
            _screenshotFormatter = screenshotFormatter;
            _screenshotSaver = screenshotSaver;
        }

        public void TakeScreenshotsFromStream()
        {
        }
        public void TakeScreenshotsFromFile(string inputPath, int timeout, int width, int height, string savePath, int threads)
        {
            var urls = _fileOpeningService.GetUrlsFromFile(inputPath);
            TakeScreenshots(urls, timeout, width, height, savePath, threads);
        }

        private void TakeScreenshots(List<string> urls, int timeout, int width, int height, string savePath, int threads)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            Parallel.ForEach(urls, new ParallelOptions() { MaxDegreeOfParallelism = threads }, (url) => TakeScreenshot(url, timeout, width, height, savePath));
            stopwatch.Stop();
            Console.WriteLine($"Elapsed: {stopwatch.Elapsed}");
        }

        private void TakeScreenshot(string url, int timeout, int width, int height, string savePath)
        {
            try
            {
                Console.WriteLine($"Start {url}");

                var driver = _webDriverFactory.CreateWebDriver(timeout);
                driver.Navigate().GoToUrl($"http://{url}");
                var ss = ((ITakesScreenshot)driver).GetScreenshot();
                var formatted = _screenshotFormatter.FormatScreenshot(ss, width, height);
                _screenshotSaver.Save(formatted, savePath, url);

                Console.WriteLine($"Done {url}");
            }
            catch (WebDriverTimeoutException)
            {
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine($"Error {url}");
                Console.ForegroundColor = ConsoleColor.White;
            }
        }
    }
}