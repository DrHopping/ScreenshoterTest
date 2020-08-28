using System.Collections.Generic;
using System.IO;
using System.Linq;
using ScreenshoterTest.Data;

namespace ScreenshoterTest.Services
{
    public interface IResultSaver
    {
        void SaveTimeoutAndErrors(ScreenshotRequestsStorage storage, string path);
    }

    public class ResultSaver : IResultSaver
    {
        private void WriteFile(IEnumerable<string> urls, string path, string filename)
        {
            using var sw = File.CreateText($"{path}/{filename}.txt");
            foreach (var url in urls)
            {
                sw.WriteLine(url);
            }
            sw.Dispose();
        }

        public void SaveTimeoutAndErrors(ScreenshotRequestsStorage storage, string path)
        {
            var timeouts = storage.Where(r => r.Result == "Timeout").Select(r => r.Url);
            var errors = storage.Where(r => r.Result == "Error").Select(r => r.Url);

            WriteFile(timeouts, path, "timeouts");
            WriteFile(errors, path, "errors");

        }
    }
}