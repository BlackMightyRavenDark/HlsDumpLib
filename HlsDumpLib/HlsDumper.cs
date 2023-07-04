﻿using System;
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
        public long ProcessedChunkCountTotal { get; private set; } = 0;
        public long ChunkDownloadErrorCount { get; private set; } = 0L;
        public long ChunkAppendErrorCount { get; private set; } = 0L;

        public long CurrentSessionFirstChunkId { get; private set; } = 0L;
        public long CurrentPlaylistFirstNewChunkId { get; private set; } = 0L;
        public int CurrentPlaylistChunkCount { get; private set; } = 0;
        public int CurrentPlaylistNewChunkCount { get; private set; } = 0;
        public long LostChunkCount { get; private set; } = 0L;

        private long _currentPlaylistFirstChunkId = -1L;
        private long _lastProcessedChunkId = -1L;
 
        private readonly LinkedList<string> _chunkUrlList = new LinkedList<string>();

        public delegate void PlaylistCheckingDelegate(object sender, string playlistUrl);
        public delegate void NextChunkDelegate(object sender, long absoluteChunkId,
            long sessionChunkId, long chunkSize, string chunkUrl);
        public delegate void DumpProgressDelegate(object sender, long fileSize, int errorCode);
        public delegate void ChunkDownloadFailedDelegate(object sender, int errorCode, long failedCount);
        public delegate void ChunkAppendFailedDelegate(object sender, long failedCount);
        public delegate void DumpMessageDelegate(object sender, string message);
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
            DumpMessageDelegate dumpMessage,
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
                const int MAX_CHECKING_INTERVAL_MILLISECONDS = 2000;
                int errorCount = 0;
                bool first = true;
                try
                {
                    using (Stream outputStream = File.OpenWrite(outputFilePath))
                    {
                        DateTime lastTime = DateTime.Now;
                        do
                        {
                            playlistChecking?.Invoke(this, Url);
                            int errorCode = playlistDownloader.DownloadString(out string response);
                            if (errorCode == 200)
                            {
                                if (first)
                                {
                                    CurrentSessionFirstChunkId = FindFirstChunkId(response);
                                    first = false;
                                }

                                List<string> unfilteredPlaylist = ParsePlaylist(response);
                                CurrentPlaylistChunkCount = unfilteredPlaylist.Count;
                                List<string> filteredPlaylist = FilterPlaylist(unfilteredPlaylist);
                                if (filteredPlaylist != null && filteredPlaylist.Count > 0)
                                {
                                    CurrentPlaylistNewChunkCount = filteredPlaylist.Count;
                                    CurrentPlaylistFirstNewChunkId = _currentPlaylistFirstChunkId + CurrentPlaylistChunkCount - CurrentPlaylistNewChunkCount;

                                    long diff = _lastProcessedChunkId >= 0L ? CurrentPlaylistFirstNewChunkId - _lastProcessedChunkId : 1L;
                                    long lost = diff - 1L;
                                    if (lost > 0)
                                    {
                                        LostChunkCount += lost;
                                        dumpError?.Invoke(this, $"Lost: {lost}, Total lost: {LostChunkCount})", -1);
                                    }

                                    for (int i = 0; i < filteredPlaylist.Count; ++i)
                                    {
                                        string chunkUrl = filteredPlaylist[i];
                                        long lastChunkLength = -1L;

                                        MemoryStream mem = new MemoryStream();
                                        FileDownloader d = new FileDownloader() { Url = chunkUrl };
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
                                            else
                                            {
                                                _lastProcessedChunkId = CurrentPlaylistFirstNewChunkId + (uint)i;
                                            }
                                        }
                                        else
                                        {
                                            ChunkDownloadErrorCount++;
                                            chunkDownloadFailed?.Invoke(this, code, ChunkDownloadErrorCount);
                                        }
                                        mem.Close();

                                        _chunkUrlList.AddLast(chunkUrl);
                                        if (_chunkUrlList.Count > 50)
                                        {
                                            _chunkUrlList.RemoveFirst();
                                        }

                                        ProcessedChunkCountTotal++;
                                        nextChunk?.Invoke(this, CurrentPlaylistFirstNewChunkId + i,
                                            ProcessedChunkCountTotal, lastChunkLength, chunkUrl);

                                        dumpProgress?.Invoke(this, outputStream.Length, code);

                                        errorCount = 0;
                                    }
                                }
                                else
                                {
                                    CurrentPlaylistNewChunkCount = 0;
                                    CurrentPlaylistFirstNewChunkId = _currentPlaylistFirstChunkId;
                                    errorCount++;
                                    dumpWarning?.Invoke(this, "No new files detected", errorCount);
                                }
                            }
                            else
                            {
                                CurrentPlaylistChunkCount = 0;
                                CurrentPlaylistNewChunkCount = 0;
                                errorCount++;
                                dumpError?.Invoke(this, "Playlist is not found", errorCount);
                            }

                            double elapsedTime = (DateTime.Now - lastTime).TotalMilliseconds;
                            int delay = MAX_CHECKING_INTERVAL_MILLISECONDS - (int)elapsedTime;
                            if (delay > 0)
                            {
                                dumpMessage?.Invoke(this,
                                    $"Waiting for {delay} milliseconds " +
                                    $"(max: {MAX_CHECKING_INTERVAL_MILLISECONDS})");
                                Thread.Sleep(delay);
                            }
                            lastTime = DateTime.Now;
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

        private List<string> ParsePlaylist(string playlist)
        {
            List<string> result = new List<string>();
            string[] strings = playlist.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);
            foreach (string str in strings)
            {
                if (str.StartsWith("#EXT-X-MEDIA-SEQUENCE:"))
                {
                    string[] splitted = str.Split(':');
                    if (uint.TryParse(splitted[1], out uint n))
                    {
                        _currentPlaylistFirstChunkId = n;
                    }
                }
                else if (str.StartsWith("http"))
                {
                    result.Add(str);
                }
            }
            return result;
        }

        private List<string> FilterPlaylist(List<string> playlist)
        {
            return playlist.Where(s => !_chunkUrlList.Contains(s))?.ToList();
        }

        private uint FindFirstChunkId(string playlist)
        {
            string[] strings = playlist.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);
            foreach (string str in strings)
            {
                if (str.StartsWith("#EXT-X-MEDIA-SEQUENCE:"))
                {
                    string[] splitted = str.Split(':');
                    return uint.TryParse(splitted[1], out uint n) ? n : 0;
                }
            }
            return 0;
        }
    }
}
