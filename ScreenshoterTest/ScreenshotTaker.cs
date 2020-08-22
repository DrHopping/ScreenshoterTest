using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using OpenQA.Selenium;
using ScreenshoterTest.Data;
using ScreenshoterTest.Services;

namespace ScreenshoterTest
{
    public interface IScreenshotTaker
    {
        void TakeScreenshotsFromStream();
        void TakeScreenshotsFromFile(ScreenshotRequestsStorage storage, int timeout, int width, int height, string savePath, int threads);

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
        public void TakeScreenshotsFromFile(ScreenshotRequestsStorage storage, int timeout, int width, int height, string savePath, int threads)
        {
            TakeScreenshots(storage, timeout, width, height, savePath, threads);
        }

        private void TakeScreenshots(ScreenshotRequestsStorage storage, int timeout, int width, int height, string savePath, int threads)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            Parallel.ForEach(storage, new ParallelOptions() { MaxDegreeOfParallelism = threads }, (req) => TakeScreenshot(req, timeout, width, height, savePath));
            stopwatch.Stop();
            Console.WriteLine($"Elapsed: {stopwatch.Elapsed}");
        }

        private void TakeScreenshot(ScreenshotRequestModel request, int timeout, int width, int height, string savePath)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            try
            {
                var driver = _webDriverFactory.CreateWebDriver(timeout);
                driver.Navigate().GoToUrl($"http://{request.Url}");
                var ss = ((ITakesScreenshot)driver).GetScreenshot();
                var formatted = _screenshotFormatter.FormatScreenshot(ss, width, height);
                _screenshotSaver.Save(formatted, savePath, request.Url);

                stopwatch.Stop();
                request.Elapsed = stopwatch.Elapsed;
                request.Result = "Done";

            }
            catch (WebDriverTimeoutException)
            {
                request.Elapsed = stopwatch.Elapsed;
                request.Result = "Error";
            }
        }
    }
}