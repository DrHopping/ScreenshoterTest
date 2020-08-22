using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace ScreenshoterTest.Services
{
    public interface IWebDriverFactory : IDisposable
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

            lock (_drivers)
            {
                _drivers.Add(Thread.CurrentThread.ManagedThreadId, driver);
            }

            return driver;
        }

        public void Dispose()
        {
            foreach (var item in _drivers)
            {
                item.Value.Dispose();
            }
        }
    }
}