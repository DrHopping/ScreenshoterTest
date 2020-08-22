using System;
using ConsoleTables;
using ScreenshoterTest.Data;

namespace ScreenshoterTest.Services
{

    public interface IConsoleWriter
    {
        void Write(ScreenshotRequestsStorage storage);
    }

    public class ConsoleWriter : IConsoleWriter
    {
        static object locker = new object();
        public void Write(ScreenshotRequestsStorage storage)
        {
            lock (locker)
            {
                Console.Clear();
                ConsoleTable
                    .From(storage)
                    .Write();
            }
        }
    }
}