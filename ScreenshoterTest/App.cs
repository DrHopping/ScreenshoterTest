using CommandDotNet;

namespace ScreenshoterTest
{
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
}