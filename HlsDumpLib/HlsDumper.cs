using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MultiThreadedDownloaderLib;

namespace HlsDumpLib
{
    public class HlsDumper
    {
        public string Url { get; }
        public uint TotalChunkCount { get; private set; } = 0;
        public uint ChunkDownloadErrorCount { get; private set; } = 0;
        public uint ChunkAppendErrorCount { get; private set; } = 0;
 
        private readonly LinkedList<string> _chunkUrlList = new LinkedList<string>();

        public delegate void PlaylistCheckingDelegate(object sender, string playlistUrl);
        public delegate void NextChunkDelegate(object sender, uint chunkNumber, long chunkSize, string chunkUrl);
        public delegate void DumpProgressDelegate(object sender, long fileSize, int errorCode);
        public delegate void ChunkDownloadFailedDelegate(object sender, int errorCode, uint failedCount);
        public delegate void ChunkAppendFailedDelegate(object sender, uint failedCount);
        public delegate void DumpWarningDelegate(object sender, string message, int errorCount);
        public delegate void DumpErrorDelegate(object sender, string message, int errorCount);
        public delegate void DumpFinishedDelegate(object sender);

        public HlsDumper(string url)
        {
            Url = url;
        }

        public async void Dump(string outputFilePath,
            PlaylistCheckingDelegate playlistChecking,
            NextChunkDelegate nextChunk,
            DumpProgressDelegate dumpProgress,
            ChunkDownloadFailedDelegate chunkDownloadFailed,
            ChunkAppendFailedDelegate chunkAppendFailed,
            DumpWarningDelegate dumpWarning,
            DumpErrorDelegate dumpError,
            DumpFinishedDelegate dumpFinished)
        {
            if (string.IsNullOrEmpty(outputFilePath) || string.IsNullOrWhiteSpace(outputFilePath))
            {
                dumpError?.Invoke(this, "No filename specified", 1);
                return;
            }

            FileDownloader playlistDownloader = new FileDownloader() { Url = Url };
            await Task.Run(() =>
            {
                int errorCount = 0;
                try
                {
                    using (Stream outputStream = File.OpenWrite(outputFilePath))
                    {
                        do
                        {
                            playlistChecking?.Invoke(this, Url);
                            int errorCode = playlistDownloader.DownloadString(out string response);
                            if (errorCode == 200)
                            {
                                List<string> list = FilterPlaylist(ExtractUrlsFromPlaylist(response));
                                if (list != null && list.Count > 0)
                                {
                                    foreach (string item in list)
                                    {
                                        long lastChunkLength = -1L;

                                        MemoryStream mem = new MemoryStream();
                                        FileDownloader d = new FileDownloader() { Url = item };
                                        int code = d.Download(mem);
                                        if (code == 200)
                                        {
                                            lastChunkLength = mem.Length;
                                            mem.Position = 0L;
                                            if (!MultiThreadedDownloader.AppendStream(mem, outputStream))
                                            {
                                                ChunkAppendErrorCount++;
                                                chunkAppendFailed?.Invoke(this, ChunkAppendErrorCount);
                                            }
                                        }
                                        else
                                        {
                                            ChunkDownloadErrorCount++;
                                            chunkDownloadFailed?.Invoke(this, code, ChunkDownloadErrorCount);
                                        }
                                        mem.Close();

                                        _chunkUrlList.AddLast(item);
                                        if (_chunkUrlList.Count > 50)
                                        {
                                            _chunkUrlList.RemoveFirst();
                                        }

                                        TotalChunkCount++;
                                        nextChunk?.Invoke(this, TotalChunkCount, lastChunkLength, item);

                                        dumpProgress?.Invoke(this, outputStream.Length, code);

                                        errorCount = 0;
                                    }
                                }
                                else
                                {
                                    errorCount++;
                                    dumpWarning?.Invoke(this, "No new files detected", errorCount);
                                }
                            }
                            else
                            {
                                errorCount++;
                                dumpError?.Invoke(this, "Playlist is not found", errorCount);
                            }

                            Thread.Sleep(2000);
                        } while (errorCount < 5);
                    }
                } catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.Message);
                    errorCount++;
                    dumpError?.Invoke(this, ex.Message, errorCount);
                }
            });

            dumpFinished?.Invoke(this);
        }

        private List<string> ExtractUrlsFromPlaylist(string playlist)
        {
            List<string> result = new List<string>();
            string[] strings = playlist.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);
            result.AddRange(strings.Where(s => s.StartsWith("http")));
            return result;
        }

        private List<string> FilterPlaylist(List<string> playlist)
        {
            return playlist.Where(s => !_chunkUrlList.Contains(s))?.ToList();
        }
    }
}
