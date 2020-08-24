using CommandDotNet;
using ScreenshoterTest.Data;
using ScreenshoterTest.Services;

namespace ScreenshoterTest
{
    public interface IApp
    {
        [DefaultMethod]
        void Execute(
            [Option(LongName = "inputFile", ShortName = "f", Description = "Input file path. Default - console stream mode.")] string inputPath,
            [Option(LongName = "savePath", ShortName = "p", Description = "Save path")] string savePath = "screenshots",
            [Option(LongName = "timeout", ShortName = "t", Description = "Page load timeout in seconds")] int timeout = 30,
            [Option(LongName = "width", ShortName = "W", Description = "Width of screenshot in px")] int width = 1920,
            [Option(LongName = "height", ShortName = "H", Description = "Height of screenshot in px")] int height = 1080,
            [Option(LongName = "threads", ShortName = "T", Description = "Number of working threads")] int threads = 4,
            [Option(LongName = "ui", ShortName = "u", Description = "Show progress table ui")] bool ui = false,
            [Option(LongName = "wait", ShortName = "w", Description = "Time to wait before making screenshot")] int wait = 0);
    }
    public class App : IApp
    {
        private readonly IScreenshotTaker _screenshotTaker;
        private readonly IFileOpeningService _fileOpeningService;
        private readonly IConsoleWriter _consoleWriter;
        private readonly IResultSaver _resultSaver;

        public App(IScreenshotTaker screenshotTaker, IFileOpeningService fileOpeningService, IConsoleWriter consoleWriter, IResultSaver resultSaver)
        {
            _screenshotTaker = screenshotTaker;
            _fileOpeningService = fileOpeningService;
            _consoleWriter = consoleWriter;
            _resultSaver = resultSaver;
        }

        public void Execute(
            string inputPath,
            string savePath = "/",
            int timeout = 30,
            int width = 1920,
            int height = 1080,
            int threads = 4,
            bool ui = false,
            int wait = 0)
        {
            if (inputPath is null)
            {
                _screenshotTaker.TakeScreenshotsFromStream();
            }
            else
            {
                var storage = new ScreenshotRequestsStorage(_fileOpeningService.GetUrlsFromFile(inputPath));
                if (ui)
                {
                    _consoleWriter.Write(storage);
                    storage.StorageEvent += (sender, args) => { _consoleWriter.Write(storage); };
                }

                _screenshotTaker.TakeScreenshotsFromFile(storage, timeout, width, height, savePath, threads, wait);
                _resultSaver.SaveTimeoutAndErrors(storage, inputPath.Contains("\\") ? inputPath.Substring(0, inputPath.LastIndexOf("\\")) : "");
            }
        }
    }
}