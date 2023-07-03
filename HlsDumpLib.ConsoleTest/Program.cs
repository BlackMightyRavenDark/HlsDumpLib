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
                    string fileName = $"hlsdumper_{DateTime.Now:yyyy-MM-dd HH-mm-ss}.ts";
                    HlsDumper dumper = new HlsDumper(url);
                    dumper.Dump(fileName, OnPlaylistChecking, OnNextChunk, OnDumpProgress,
                        OnChunkDownloadFailed, OnChunkAppendFailed, OnWarning, OnError, OnFinished);
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

        private static void OnNextChunk(object sender, uint absoluteChunkNumber,
            uint sessionChunkNumber, long chunkSize, string chunkFileUrl)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write($"Chunk №{sessionChunkNumber} ({absoluteChunkNumber}): ");
            Console.ForegroundColor = ConsoleColor.White;
            string t = chunkSize >= 0L ? $"{chunkFileUrl}, {chunkSize} bytes" : chunkFileUrl;
            Console.WriteLine(t);
        }

        private static void OnDumpProgress(object sender, long fileSize, int errorCode)
        {
            if (errorCode == 200)
            {
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine($"Dumped file size: {fileSize} bytes");
                HlsDumper dumper = sender as HlsDumper;
                if (dumper.ChunkDownloadErrorCount > 0)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Chunk download errors: {dumper.ChunkDownloadErrorCount}");
                }
                if (dumper.ChunkAppendErrorCount > 0)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Chunk append errors: {dumper.ChunkAppendErrorCount}");
                }
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"HTTP error {errorCode}!");
            }
        }

        private static void OnChunkDownloadFailed(object sender, int errorCode, uint failedCount)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            uint number = (sender as HlsDumper).ProcessedChunkCountTotal;
            Console.WriteLine($"Chunk №{number} download failed with error code {errorCode}! " +
                $"Total similar errors: {failedCount}");
        }

        public static void OnChunkAppendFailed(object sender, uint failedCount)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            uint number = (sender as HlsDumper).ProcessedChunkCountTotal;
            Console.WriteLine($"Chunk №{number} append failed! Total similar errors: {failedCount}");
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
