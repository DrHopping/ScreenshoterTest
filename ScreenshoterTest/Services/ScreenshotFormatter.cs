using System.Drawing;
using System.IO;
using OpenQA.Selenium;

namespace ScreenshoterTest.Services
{
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
}