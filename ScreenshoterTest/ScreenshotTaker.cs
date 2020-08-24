using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using ScreenshoterTest.Data;
using ScreenshoterTest.Helpers;
using ScreenshoterTest.Services;

namespace ScreenshoterTest
{
    public interface IScreenshotTaker
    {
        void TakeScreenshotsFromStream();
        void TakeScreenshotsFromFile(ScreenshotRequestsStorage storage, int timeout, int width, int height, string savePath, int threads, int wait);

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
        public void TakeScreenshotsFromFile(ScreenshotRequestsStorage storage, int timeout, int width, int height, string savePath, int threads, int wait)
        {
            TakeScreenshots(storage, timeout, width, height, savePath, threads, wait);
        }

        private void TakeScreenshots(ScreenshotRequestsStorage storage, int timeout, int width, int height, string savePath, int threads, int wait)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            Parallel.ForEach(storage, new ParallelOptions()
            {
                MaxDegreeOfParallelism = threads
            }, (req) => TakeScreenshot(req, timeout, width, height, savePath, wait));

            stopwatch.Stop();
            Console.WriteLine($"Elapsed: {stopwatch.Elapsed}");
            _webDriverFactory.Dispose();
        }

        private void TakeScreenshot(ScreenshotRequestModel request, int timeout, int width, int height, string savePath, int wait)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            try
            {
                Func<Screenshot> func = () =>
                {
                    try
                    {
                        var driver = _webDriverFactory.CreateWebDriver(timeout);
                        driver.Navigate().GoToUrl($"http://{request.Url}");
                        driver.Wait(TimeSpan.FromSeconds(wait));
                        var ss = ((ITakesScreenshot)driver).GetScreenshot();
                        return ss;
                    }
                    catch
                    {
                        return null;
                    }
                };
                var task = Task.Run(func);

                if (task.Wait(TimeSpan.FromSeconds(timeout + wait)))
                {
                    var ss = task.Result;
                    if (ss == null) throw new Exception();
                    var formatted = _screenshotFormatter.FormatScreenshot(ss, width, height);
                    _screenshotSaver.Save(formatted, savePath, request.Url);
                }
                else
                {
                    throw new TimeoutException("Timeout");
                }

                stopwatch.Stop();
                request.Elapsed = stopwatch.Elapsed;
                request.Result = "Done";

            }
            catch (TimeoutException)
            {
                stopwatch.Stop();
                request.Elapsed = stopwatch.Elapsed;
                request.Result = "Timeout";
            }
            catch
            {
                stopwatch.Stop();
                request.Elapsed = stopwatch.Elapsed;
                request.Result = "Error";
            }
        }
    }
}