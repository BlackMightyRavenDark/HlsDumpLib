using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MultiThreadedDownloaderLib;
using Newtonsoft.Json.Linq;

namespace HlsDumpLib
{
    public class HlsDumper : IDisposable
    {
        public string Url { get; }
        public long ProcessedChunkCountTotal { get; private set; } = 0L;
        public long ChunkDownloadErrorCount { get; private set; } = 0L;
        public long ChunkAppendErrorCount { get; private set; } = 0L;

        public long CurrentSessionFirstChunkId { get; private set; } = 0L;
        public long CurrentPlaylistFirstNewChunkId { get; private set; } = 0L;
        public int CurrentPlaylistChunkCount { get; private set; } = 0;
        public int CurrentPlaylistNewChunkCount { get; private set; } = 0;
        public long LostChunkCount { get; private set; } = 0L;
        public int LastDelayValueMilliseconds { get; private set; } = 0;

        private long _currentPlaylistFirstChunkId = -1L;
        private long _lastProcessedChunkId = -1L;

        private readonly LinkedList<string> _chunkUrlList = new LinkedList<string>();

        private CancellationTokenSource _cancellationTokenSource;
        private CancellationToken _cancellationToken;

        public const int DUMPING_ERROR_PLAYLIST_GONE = -1;
        public const int DUMPING_ERROR_CANCELED = -2;

        public delegate void PlaylistCheckingDelegate(object sender, string playlistUrl);
        public delegate void PlaylistCheckedDelegate(object sender, int errorCode);
        public delegate void PlaylistFirstArrived(object sender, int chunkCount, long firstChunkId);
        public delegate void NextChunkDelegate(object sender, long absoluteChunkId,
            long sessionChunkId, long chunkSize, string chunkUrl);
        public delegate void DumpProgressDelegate(object sender, long fileSize, int errorCode);
        public delegate void ChunkDownloadFailedDelegate(object sender, int errorCode, long failedCount);
        public delegate void ChunkAppendFailedDelegate(object sender, long failedCount);
        public delegate void DumpMessageDelegate(object sender, string message);
        public delegate void DumpWarningDelegate(object sender, string message, int errorCount);
        public delegate void DumpErrorDelegate(object sender, string message, int errorCount);
        public delegate void DumpFinishedDelegate(object sender, int errorCode);

        public HlsDumper(string url)
        {
            Url = url;
        }

        public void Dispose()
        {
            if (_cancellationTokenSource != null)
            {
                _cancellationTokenSource.Dispose();
                _cancellationTokenSource = null;
            }
        }

        public async void Dump(string outputFilePath,
            PlaylistCheckingDelegate playlistChecking,
            PlaylistCheckedDelegate playlistChecked,
            PlaylistFirstArrived playlistFirstArrived,
            NextChunkDelegate nextChunk,
            DumpProgressDelegate dumpProgress,
            ChunkDownloadFailedDelegate chunkDownloadFailed,
            ChunkAppendFailedDelegate chunkAppendFailed,
            DumpMessageDelegate dumpMessage,
            DumpWarningDelegate dumpWarning,
            DumpErrorDelegate dumpError,
            DumpFinishedDelegate dumpFinished,
            bool writeChunksInfo,
            bool breakIfPlaylistLost)
        {
            if (string.IsNullOrEmpty(outputFilePath) || string.IsNullOrWhiteSpace(outputFilePath))
            {
                dumpError?.Invoke(this, "No filename specified", 1);
                return;
            }

            _cancellationTokenSource = new CancellationTokenSource();
            _cancellationToken = _cancellationTokenSource.Token;

            JArray jChunks = new JArray();
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
                        do
                        {
                            int timeStart = Environment.TickCount;
                            playlistChecking?.Invoke(this, Url);

                            List<string> unfilteredPlaylist = null;
                            List<string> filteredPlaylist = null;
                            int playlistErrorCode = playlistDownloader.DownloadString(out string response);
                            if (playlistErrorCode == 200)
                            {
                                unfilteredPlaylist = ParsePlaylist(response);
                                CurrentPlaylistChunkCount = unfilteredPlaylist.Count;

                                if (first)
                                {
                                    first = false;
                                    CurrentSessionFirstChunkId = FindPlaylistFirstChunkId(response);
                                    playlistFirstArrived?.Invoke(this, CurrentPlaylistChunkCount, CurrentSessionFirstChunkId);
                                }

                                filteredPlaylist = FilterPlaylist(unfilteredPlaylist);
                                if (filteredPlaylist != null)
                                {
                                    CurrentPlaylistNewChunkCount = filteredPlaylist.Count;
                                    CurrentPlaylistFirstNewChunkId = _currentPlaylistFirstChunkId +
                                        CurrentPlaylistChunkCount - CurrentPlaylistNewChunkCount;
                                }
                                else
                                {
                                    CurrentPlaylistNewChunkCount = 0;
                                    CurrentPlaylistFirstNewChunkId = -1L;
                                }

                                playlistChecked?.Invoke(this, playlistErrorCode);

                                if (CurrentPlaylistNewChunkCount == 0)
                                {
                                    errorCount++;
                                    dumpWarning?.Invoke(this, "No new files detected", errorCount);
                                }

                                long diff = _lastProcessedChunkId >= 0L ? CurrentPlaylistFirstNewChunkId - _lastProcessedChunkId : 1L;
                                long lost = diff - 1L;
                                if (lost > 0)
                                {
                                    LostChunkCount += lost;
                                    dumpError?.Invoke(this, $"Lost: {lost}, Total lost: {LostChunkCount})", -1);
                                }
                            }
                            else
                            {
                                playlistChecked?.Invoke(this, playlistErrorCode);

                                errorCount++;

                                if (breakIfPlaylistLost)
                                {
                                    dumpError?.Invoke(this, "Playlist lost! Breaking...", -1);
                                    break;
                                }
                                else if (errorCount >= 5)
                                {
                                    break;
                                }
                            }

                            if (playlistErrorCode == 200)
                            {
                                if (filteredPlaylist != null && filteredPlaylist.Count > 0)
                                {
                                    for (int i = 0; i < filteredPlaylist.Count; ++i)
                                    {
                                        string chunkUrl = filteredPlaylist[i];
                                        long lastChunkLength = -1L;
                                        long currentAbsoluteChunkId = CurrentPlaylistFirstNewChunkId + i;

                                        MemoryStream mem = new MemoryStream();
                                        FileDownloader d = new FileDownloader() { Url = chunkUrl };
                                        int code = d.Download(mem);
                                        if (code == 200)
                                        {
                                            lastChunkLength = mem.Length;
                                            mem.Position = 0L;
                                            if (MultiThreadedDownloader.AppendStream(mem, outputStream))
                                            {
                                                _lastProcessedChunkId = currentAbsoluteChunkId;
                                                if (writeChunksInfo)
                                                {
                                                    JObject jChunk = new JObject();
                                                    jChunk["position"] = outputStream.Position - mem.Length;
                                                    jChunk["size"] = mem.Length;
                                                    jChunk["id"] = currentAbsoluteChunkId;
                                                    //TODO: Determine and store other chunk information from playlist
                                                    jChunks.Add(jChunk);
                                                }
                                            }
                                            else
                                            {
                                                ChunkAppendErrorCount++;
                                                chunkAppendFailed?.Invoke(this, ChunkAppendErrorCount);
                                                //TODO: The stream and chunks information data will be corrupted here, so it's strongly needed to do some magic thing!
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
                                        nextChunk?.Invoke(this, currentAbsoluteChunkId,
                                            ProcessedChunkCountTotal, lastChunkLength, chunkUrl);

                                        dumpProgress?.Invoke(this, outputStream.Length, code);

                                        errorCount = 0;

                                        if (_cancellationToken.IsCancellationRequested) { break; }
                                    }
                                }
                            }

                            if (_cancellationToken.IsCancellationRequested) { break; }

                            int elapsedTime = Environment.TickCount - timeStart;
                            LastDelayValueMilliseconds = MAX_CHECKING_INTERVAL_MILLISECONDS - elapsedTime;
                            if (LastDelayValueMilliseconds > 0)
                            {
                                dumpMessage?.Invoke(this,
                                    $"Waiting for {LastDelayValueMilliseconds} milliseconds " +
                                    $"(max: {MAX_CHECKING_INTERVAL_MILLISECONDS})");
                                Thread.Sleep(LastDelayValueMilliseconds);
                            }
                        } while (errorCount < 5 && !_cancellationToken.IsCancellationRequested);
                    }

                    if (writeChunksInfo)
                    {
                        JObject json = new JObject();
                        json["playlistUrl"] = Url;
                        json["outputFile"] = outputFilePath;
                        json.Add(new JProperty("chunks", jChunks));
                        File.WriteAllText($"{outputFilePath}_chunks.json", json.ToString());
                    }
                } catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.Message);
                    errorCount++;
                    dumpError?.Invoke(this, ex.Message, errorCount);
                }
            });

            int e = _cancellationToken.IsCancellationRequested ? DUMPING_ERROR_CANCELED : DUMPING_ERROR_PLAYLIST_GONE;
            dumpFinished?.Invoke(this, e);
        }

        public void StopDumping()
        {
            if (_cancellationTokenSource != null && !_cancellationTokenSource.IsCancellationRequested)
            {
                _cancellationTokenSource.Cancel();
            }
        }

        private List<string> ParsePlaylist(string playlist)
        {
            List<string> result = new List<string>();
            string[] strings = playlist.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);
            _currentPlaylistFirstChunkId = FindPlaylistFirstChunkId(strings);
            foreach (string str in strings)
            {
                if (str.StartsWith("http"))
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

        private long FindPlaylistFirstChunkId(string[] playlistStrings)
        {
            foreach (string str in playlistStrings)
            {
                if (str.StartsWith("#EXT-X-MEDIA-SEQUENCE:"))
                {
                    string[] splitted = str.Split(':');
                    return long.TryParse(splitted[1], out long n) ? n : 0L;
                }
            }
            return -1L;
        }

        private long FindPlaylistFirstChunkId(string playlist)
        {
            string[] strings = playlist.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);
            return FindPlaylistFirstChunkId(strings);
        }
    }
}
