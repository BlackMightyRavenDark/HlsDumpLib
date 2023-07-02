using System;
using System.IO;

namespace HlsDumpLib.ConsoleTest
{
    internal class Program
    {
        static void Main(string[] args)
        {
            byte[] inputBuffer = new byte[8192];
            Stream inputStream = Console.OpenStandardInput(inputBuffer.Length);
            Console.SetIn(new StreamReader(inputStream, Console.InputEncoding, false, inputBuffer.Length));

            Console.WriteLine("Enter HLS-playlist URL (M3U8). Warning! Not all playlist formats are supported yet!");
            Console.Write("URL: ");
            string url = Console.ReadLine();
            if (!string.IsNullOrEmpty(url) && !string.IsNullOrWhiteSpace(url))
            {
                int errorCode = MultiThreadedDownloaderLib.FileDownloader.GetUrlResponseHeaders(url, out _, out string errorText);
                if (errorCode == 200)
                {
                    HlsDumper dumper = new HlsDumper(url);
                    dumper.Dump(OnPlaylistChecking, OnNextChunk, OnWarning, OnError, OnFinished);
                }
                else
                {
                    Console.WriteLine($"Error {errorCode}! {errorText}");
                }
            }
            else
            {
                Console.WriteLine("Empty URL! Exiting... Press any key...");
            }
            Console.ReadLine();
        }

        private static void OnPlaylistChecking(object sender, string playlistFileUrl)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("Checking playlist: ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(playlistFileUrl);
        }

        private static void OnNextChunk(object sender, uint chunkNumber, string chunkFileUrl)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write($"Chunk №{chunkNumber}: ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(chunkFileUrl);
        }

        private static void OnWarning(object sender, string message, int errorCount)
        {
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine($"Error {errorCount} / 5: {message}");
        }

        private static void OnError(object sender, string message, int errorCount)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Error {errorCount} / 5: {message}");
        }

        private static void OnFinished(object sender)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("Dump is finished");
        }
    }
}
