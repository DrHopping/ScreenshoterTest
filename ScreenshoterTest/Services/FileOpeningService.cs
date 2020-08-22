using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ScreenshoterTest.Services
{
    public interface IFileOpeningService
    {
        List<string> GetUrlsFromFile(string path);
    }
    public class FileOpeningService : IFileOpeningService
    {
        public List<string> GetUrlsFromFile(string path)
        {
            List<string> urls = File.ReadAllLines(path).ToList();
            return urls;
        }
    }
}