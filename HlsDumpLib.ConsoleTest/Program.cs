﻿using System;
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
                int errorCode = MultiThreadedDownloaderLib.FileDownloader.GetUrlResponseHeaders(url, null, out _, out string errorText);
                if (errorCode == 200)
                {
                    const bool useGmtTime = true;
                    string outputFileName = useGmtTime ?
                        $"hlsdump_{DateTime.UtcNow:yyyy-MM-dd HH-mm-ss} GMT" :
                        $"hlsdump_{DateTime.Now:yyyy-MM-dd HH-mm-ss}";

                    HlsDumper dumper = new HlsDumper(url);
                    dumper.Dump(outputFileName, OnPlaylistCheckingStarted, OnPlaylistCheckingFinished, null, null, null, null,
                        OnNextChunkConnecting, OnNextChunkConnected, OnNextChunkProcessed, null,
                        OnDumpProgress, OnChunkDownloadFailed, OnChunkAppendFailed,
                        OnMessage, OnWarning, OnError, OnFinished,
                        2000, 5, 5, true, true, true, useGmtTime);
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

        private static void OnPlaylistCheckingStarted(object sender, string playlistFileUrl)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("Checking playlist: ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(playlistFileUrl);
        }

        private static void OnPlaylistCheckingFinished(object sender,
            int chunkCount, int newChunkCount, int firstChunkId, int firstNewChunkId,
            string playlistContent, int errorCode, int playlistErrorCountInRow)
        {
            if (errorCode == 200)
            {
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine($"Playlist total chunks: {chunkCount}");
                Console.WriteLine($"Playlist new chunks: {newChunkCount}");
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Playlist checking failed! Error code: {errorCode}, " +
                    $"Error count: {playlistErrorCountInRow} / {(sender as HlsDumper).PlaylistErrorCountInRowMax}");
            }
        }

        private static void OnNextChunkConnecting(object sender, StreamSegment chunk)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine($"Connecting to the next chunk...");
        }

        private static void OnNextChunkConnected(object sender, StreamSegment chunk, long chunkFileSize, int errorCode)
        {
            if (errorCode == 200)
            {
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine($"Downloading chunk ({chunkFileSize} bytes)...");
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Can not connect to the next chunk!");
            }
        }

        private static void OnNextChunkProcessed(object sender, StreamSegment chunk,
            long chunkSize, int sessionChunkId, int chunkProcessingTime)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write($"Chunk {sessionChunkId} ({chunk.Id}): ");
            Console.ForegroundColor = ConsoleColor.White;
            string t = chunkSize >= 0L ? $"{chunk.Url}, {chunkSize} bytes" : chunk.Url;
            Console.WriteLine(t);
            Console.WriteLine($"Chunk processing time: {chunkProcessingTime}ms");
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
                if (dumper.LostChunkCount > 0)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Lost chunk count: {dumper.LostChunkCount}");
                }
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"HTTP error {errorCode}!");
            }
        }

        private static void OnChunkDownloadFailed(object sender, int errorCode, int failedCount)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            int number = (sender as HlsDumper).ProcessedChunkCountTotal;
            Console.WriteLine($"Chunk №{number} download failed with error code {errorCode}! " +
                $"Total similar errors: {failedCount}");
        }

        public static void OnChunkAppendFailed(object sender, int failedCount)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            int number = (sender as HlsDumper).ProcessedChunkCountTotal;
            Console.WriteLine($"Chunk №{number} append failed! Total similar errors: {failedCount}");
        }

        private static void OnMessage(object sender, string message)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(message);
        }

        private static void OnWarning(object sender, string message, int errorCount)
        {
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine($"Warning {errorCount} / 5: {message}");
        }

        private static void OnError(object sender, string message, int errorCount)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(errorCount >= 0 ? $"Error {errorCount} / 5: {message}" : message);
        }

        private static void OnFinished(object sender, int errorCode)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("Dump is finished");
        }
    }
}
