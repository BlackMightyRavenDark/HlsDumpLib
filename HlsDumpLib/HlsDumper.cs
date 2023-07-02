using System;
using System.Collections.Generic;
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
 
        private readonly LinkedList<string> _chunkUrlList = new LinkedList<string>();

        public delegate void PlaylistCheckingDelegate(object sender, string playlistUrl);
        public delegate void NextChunkDelegate(object sender, uint chunkNumber, string chunkUrl);
        public delegate void DumpWarningDelegate(object sender, string message, int errorCount);
        public delegate void DumpErrorDelegate(object sender, string message, int errorCount);
        public delegate void DumpFinishedDelegate(object sender);

        public HlsDumper(string url)
        {
            Url = url;
        }

        public async void Dump(
            PlaylistCheckingDelegate playlistChecking,
            NextChunkDelegate nextChunk,
            DumpWarningDelegate dumpWarning,
            DumpErrorDelegate dumpError,
            DumpFinishedDelegate dumpFinished)
        {
            FileDownloader playlistDownloader = new FileDownloader() { Url = Url };
            await Task.Run(() =>
            {
                int errorCount = 0;
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
                                _chunkUrlList.AddLast(item);
                                if (_chunkUrlList.Count > 50)
                                {
                                    _chunkUrlList.RemoveFirst();
                                }

                                TotalChunkCount++;
                                nextChunk?.Invoke(this, TotalChunkCount, item);
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
