using System;
using System.Threading;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace ScreenshoterTest.Helpers
{
    public static class WebDriverExtensions
    {
        public static void Wait(this IWebDriver driver, TimeSpan seconds)
        {
            if (seconds >= TimeSpan.FromSeconds(10))
            {
                WebDriverWait driverWait = new WebDriverWait(driver, seconds / 2);
                driverWait.Until(ExpectedConditions.ElementExists(By.TagName("div")));
                Thread.Sleep(seconds / 2);
            }
            else
            {
                WebDriverWait driverWait = new WebDriverWait(driver, TimeSpan.FromSeconds(5) + seconds);
                driverWait.Until(ExpectedConditions.ElementExists(By.TagName("div")));

                if (seconds == TimeSpan.FromSeconds(0)) return;
                Thread.Sleep(seconds);
            }



        }
    }
}