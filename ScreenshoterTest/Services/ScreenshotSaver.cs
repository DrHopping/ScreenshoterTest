using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace ScreenshoterTest.Services
{
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
}