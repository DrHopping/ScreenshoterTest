using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using CommandDotNet;
using CommandDotNet.Builders;
using CommandDotNet.IoC.MicrosoftDependencyInjection;
using ConsoleTables;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
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

            //Test();

            return new AppRunner<IApp>().UseMicrosoftDependencyInjection(host.Services).Run(args);
        }

        private static void Test()
        {
            var a = new ScreenshotRequestsStorage()
            {
                new ScreenshotRequestModel{Url = "pornhub.com"},
                new ScreenshotRequestModel{Url = "youtube.com"},
                new ScreenshotRequestModel{Url = "google.com"},
            };

            a.StorageEvent += (sender, args) => { Console.WriteLine(123); };

            a.First(s => s.Url == "pornhub.com").Result = "Error";

        }
    }

    public interface IApp
    {
        [DefaultMethod]
        void Execute(
            [Option(LongName = "inputFile", ShortName = "f", Description = "Input file path. Default - console stream mode.")] string inputPath,
            [Option(LongName = "savePath", ShortName = "p", Description = "Save path")] string savePath = "screenshots",
            [Option(LongName = "timeout", ShortName = "t", Description = "Page load timeout in seconds")] int timeout = 20,
            [Option(LongName = "width", ShortName = "W", Description = "Width of screenshot in px")] int width = 1920,
            [Option(LongName = "height", ShortName = "H", Description = "Height of screenshot in px")] int height = 1080,
            [Option(LongName = "threads", ShortName = "T", Description = "Number of working threads")] int threads = 5);
    }

    public class App : IApp
    {
        private readonly IScreenshotTaker _screenshotTaker;

        public App(IScreenshotTaker screenshotTaker)
        {
            _screenshotTaker = screenshotTaker;
        }

        public void Execute(
            string inputPath,
            string savePath = "/",
            int timeout = 20,
            int width = 1920,
            int height = 1080,
            int threads = 5)
        {
            if (inputPath is null)
                _screenshotTaker.TakeScreenshotsFromStream();
            else
                _screenshotTaker.TakeScreenshotsFromFile(inputPath, timeout, width, height, savePath, threads);

        }
    }

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

    public interface IWebDriverFactory
    {
        IWebDriver CreateWebDriver(int timeout);
    }

    class WebDriverFactory : IWebDriverFactory
    {
        private Dictionary<int, IWebDriver> _drivers = new Dictionary<int, IWebDriver>();
        private ChromeOptions _options;
        private ChromeDriverService _service;
        public WebDriverFactory()
        {
            var options = new ChromeOptions();
            options.AddArgument("--headless");
            _options = options;

            var service = ChromeDriverService.CreateDefaultService();
            service.HideCommandPromptWindow = true;
            _service = service;

        }
        public IWebDriver CreateWebDriver(int timeout)
        {
            if (_drivers.ContainsKey(Thread.CurrentThread.ManagedThreadId))
                return _drivers[Thread.CurrentThread.ManagedThreadId];

            IWebDriver driver = new ChromeDriver(_service, _options);
            driver.Manage().Window.Size = new Size(1920, 1080);
            driver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(timeout);

            lock (_drivers)
            {
                _drivers.Add(Thread.CurrentThread.ManagedThreadId, driver);
            }

            return driver;
        }
    }

    public interface IScreenshotFormatter
    {
        Bitmap FormatScreenshot(Screenshot screenshot, int width, int height);
    }

    class ScreenshotFormatter : IScreenshotFormatter
    {
        public Bitmap FormatScreenshot(Screenshot screenshot, int width, int height)
        {
            using (MemoryStream memstr = new MemoryStream(screenshot.AsByteArray))
            {
                Image image = Image.FromStream(memstr);
                var bitmap = new Bitmap(image, width, height);
                return bitmap;
            }
        }
    }

    public interface IScreenshotSaver
    {
        void Save(Bitmap screenshot, string path, string url);
    }

    class ScreenshotSaver : IScreenshotSaver
    {
        public void Save(Bitmap screenshot, string path, string url)
        {
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            screenshot.Save($"{path}/{url}.png", ImageFormat.Png);
        }
    }

    class ScreenshotRequestModel
    {
        #region EventHandler

        public delegate void RequestModelEventHandler(object sender, EventArgs e);

        public event RequestModelEventHandler ModelChangeEvent;
        protected virtual void RaiseEvent()
        {
            ModelChangeEvent?.Invoke(this, EventArgs.Empty);
        }

        #endregion

        #region Properties

        private TimeSpan _startedAt;
        private TimeSpan _finishedAt;
        private string _result;

        public string Url { get; set; }

        public TimeSpan StartedAt
        {
            get => _startedAt;
            set { _startedAt = value; RaiseEvent(); }
        }

        public TimeSpan FinishedAt
        {
            get => _finishedAt;
            set { _finishedAt = value; RaiseEvent(); }
        }

        public string Result
        {
            get => _result;
            set { _result = value; RaiseEvent(); }
        }

        #endregion

    }

    class ScreenshotRequestsStorage : ICollection<ScreenshotRequestModel>
    {
        private List<ScreenshotRequestModel> _requests;

        public ScreenshotRequestsStorage()
        {
            _requests = new List<ScreenshotRequestModel>();
        }
        #region EventHandler

        public delegate void StorageEventHandler(object sender, EventArgs e);

        public event StorageEventHandler StorageEvent;

        protected virtual void RaiseEvent(object sender, EventArgs e)
        {
            StorageEvent?.Invoke(this, EventArgs.Empty);
        }

        #endregion

        #region IColletion

        public IEnumerator<ScreenshotRequestModel> GetEnumerator()
        {
            return _requests.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(ScreenshotRequestModel item)
        {
            _requests.Add(item);
            item.ModelChangeEvent += RaiseEvent;
        }

        public void Clear()
        {
            _requests.Clear(); ;
        }

        public bool Contains(ScreenshotRequestModel item)
        {
            return _requests.Contains(item);
        }

        public void CopyTo(ScreenshotRequestModel[] array, int arrayIndex)
        {
            _requests.CopyTo(array, arrayIndex);
        }

        public bool Remove(ScreenshotRequestModel item)
        {
            return _requests.Remove(item);
        }

        public int Count => _requests.Count;

        public bool IsReadOnly => false;

        #endregion
    }

}
