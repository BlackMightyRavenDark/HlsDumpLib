using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using MultiThreadedDownloaderLib;
using System.Linq;

namespace HlsDumpLib
{
    public class HlsDumper : IDisposable
    {
        public string Url { get; }
        public string ActualUrl { get; set; }
        public long ProcessedChunkCountTotal { get; private set; } = 0L;
        public long ChunkDownloadErrorCount { get; private set; } = 0L;
        public long ChunkAppendErrorCount { get; private set; } = 0L;

        public long CurrentSessionFirstChunkId { get; private set; } = 0L;
        public long CurrentPlaylistFirstNewChunkId { get; private set; } = 0L;
        public int CurrentPlaylistChunkCount { get; private set; } = 0;
        public int CurrentPlaylistNewChunkCount { get; private set; } = 0;
        public long LostChunkCount { get; private set; } = 0L;
        public int LastDelayValueMilliseconds { get; private set; } = 0;
        public int PlaylistErrorCountInRowMax { get; private set; } = 5;
        public int PlaylistErrorCountInRow { get; private set; } = 0;
        public int OtherErrorCountInRowMax { get; private set; } = 5;
        public int OtherErrorCountInRow { get; private set; } = 0;

        public int PlaylistCheckingIntervalMilliseconds { get; private set; } = 2000;

        private long _currentPlaylistFirstChunkId = -1L;
        private long _lastProcessedChunkId = -1L;

        private readonly LinkedList<string> _chunkUrlList = new LinkedList<string>();

        private CancellationTokenSource _cancellationTokenSource;
        private CancellationToken _cancellationToken;

        public const int DUMPING_ERROR_PLAYLIST_GONE = -1;
        public const int DUMPING_ERROR_CANCELED = -2;

        public delegate void PlaylistCheckingDelegate(object sender, string playlistUrl);
        public delegate void PlaylistCheckedDelegate(object sender,
            int chunkCount, int newChunkCount, long firstChunkId, long firstNewChunkId,
            string playlistContent, int errorCode, int playlistErrorCountInRow);
        public delegate void PlaylistFirstArrivedDelegate(object sender, int chunkCount, long firstChunkId);
        public delegate void PlaylistCheckingDelayCalculatedDelegate(object sender,
            int delay, int checkingInterval, int cycleProcessingTime);
        public delegate void NextChunkArrivedDelegate(object sender, long absoluteChunkId, long sessionChunkId,
            long chunkSize, int chunkProcessingTime, string chunkUrl);
        public delegate void UpdateErrorsDelegate(object sender,
            int playlistErrorCountInRow, int playlistErrorCountInRowMax,
            int otherErrorCountInRow, int otherErrorCountInRowMax,
            long chunkDownloadErrorCount, long chunkAppendErrorCount,
            long lostChunkCount);
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
            PlaylistFirstArrivedDelegate playlistFirstArrived,
            PlaylistCheckingDelayCalculatedDelegate playlistCheckingDelayCalculated,
            NextChunkArrivedDelegate nextChunkArrived,
            UpdateErrorsDelegate updateErrors,
            DumpProgressDelegate dumpProgress,
            ChunkDownloadFailedDelegate chunkDownloadFailed,
            ChunkAppendFailedDelegate chunkAppendFailed,
            DumpMessageDelegate dumpMessage,
            DumpWarningDelegate dumpWarning,
            DumpErrorDelegate dumpError,
            DumpFinishedDelegate dumpFinished,
            int playlistCheckingIntervalMilliseconds,
            bool writeChunksInfo,
            int maxPlaylistErrorCountInRow,
            int maxOtherErrorsInRow)
        {
            if (string.IsNullOrEmpty(outputFilePath) || string.IsNullOrWhiteSpace(outputFilePath))
            {
                dumpError?.Invoke(this, "No filename specified", 1);
                return;
            }

            _cancellationTokenSource = new CancellationTokenSource();
            _cancellationToken = _cancellationTokenSource.Token;

            PlaylistCheckingIntervalMilliseconds =
                playlistCheckingIntervalMilliseconds >= 200 ? playlistCheckingIntervalMilliseconds : 2000;
            PlaylistErrorCountInRowMax = maxPlaylistErrorCountInRow;
            OtherErrorCountInRowMax = maxOtherErrorsInRow <= 0 ? 5 : maxOtherErrorsInRow;
            PlaylistErrorCountInRow = OtherErrorCountInRow = 0;

            await Task.Run(() =>
            {
                bool first = true;

                JArray jChunks = new JArray();
                FileDownloader playlistDownloader = new FileDownloader() { Url = Url };

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
                                PlaylistErrorCountInRow = 0;

                                M3UPlaylist playlist = new M3UPlaylist(response);
                                playlist.Parse();

                                if (first)
                                {
                                    first = false;
                                    if (playlist.SubPlaylistUrls != null && playlist.SubPlaylistUrls.Count > 0)
                                    {
                                        ActualUrl = playlist.SubPlaylistUrls[0];
                                        playlistDownloader.Url = ActualUrl;
                                        playlistErrorCode = playlistDownloader.DownloadString(out response);
                                        if (playlistErrorCode != 200)
                                        {
                                            OtherErrorCountInRow++;
                                            dumpError?.Invoke(this, "Failed to download playlist", OtherErrorCountInRow);
                                            break;
                                        }
                                        playlist = new M3UPlaylist(response);
                                        playlist.Parse();
                                    }

                                    CurrentSessionFirstChunkId = playlist.MediaSequence >= 0 ? playlist.MediaSequence : 0L;
                                    playlistFirstArrived?.Invoke(this, CurrentPlaylistChunkCount, CurrentSessionFirstChunkId);
                                }

                                _currentPlaylistFirstChunkId = playlist.MediaSequence >= 0 ? playlist.MediaSequence : 0L;

                                unfilteredPlaylist = new List<string>();
                                if (playlist.Segments != null)
                                {
                                    unfilteredPlaylist.AddRange(playlist.Segments);
                                }
                                CurrentPlaylistChunkCount = unfilteredPlaylist.Count;

                                filteredPlaylist = playlist.Filter(_chunkUrlList)?.ToList();
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

                                long diff = _lastProcessedChunkId >= 0L ? CurrentPlaylistFirstNewChunkId - _lastProcessedChunkId : 1L;
                                long lost = diff - 1L;
                                if (lost > 0)
                                {
                                    LostChunkCount += lost;
                                    OtherErrorCountInRow++;
                                    dumpError?.Invoke(this, $"Lost: {lost}, Total lost: {LostChunkCount})", -1);
                                }

                                playlistChecked?.Invoke(this,
                                    CurrentPlaylistChunkCount, CurrentPlaylistNewChunkCount,
                                    _currentPlaylistFirstChunkId, CurrentPlaylistFirstNewChunkId,
                                    response, playlistErrorCode, PlaylistErrorCountInRow);

                                if (CurrentPlaylistNewChunkCount == 0)
                                {
                                    OtherErrorCountInRow++;
                                    dumpWarning?.Invoke(this, "No new files detected", OtherErrorCountInRow);
                                }
                                else
                                {
                                    OtherErrorCountInRow = 0;
                                }
                            }
                            else
                            {
                                PlaylistErrorCountInRow++;

                                playlistChecked?.Invoke(this,
                                    CurrentPlaylistChunkCount, CurrentPlaylistNewChunkCount,
                                    _currentPlaylistFirstChunkId, CurrentPlaylistFirstNewChunkId,
                                    response, playlistErrorCode, PlaylistErrorCountInRow);

                                if (PlaylistErrorCountInRowMax > 0)
                                {
                                    if (PlaylistErrorCountInRow >= PlaylistErrorCountInRowMax)
                                    {
                                        dumpError?.Invoke(this,
                                            "Playlist lost! Max error count limit is reached! Breaking...",
                                            PlaylistErrorCountInRow);
                                        updateErrors?.Invoke(this, PlaylistErrorCountInRow, PlaylistErrorCountInRowMax,
                                            OtherErrorCountInRow, OtherErrorCountInRowMax, ChunkDownloadErrorCount,
                                            ChunkAppendErrorCount, LostChunkCount);
                                        break;
                                    }
                                    else
                                    {
                                        dumpError?.Invoke(this,
                                            $"Playlist lost {PlaylistErrorCountInRow} / {PlaylistErrorCountInRowMax}!",
                                            PlaylistErrorCountInRow);
                                    }
                                }
                                else
                                {
                                    dumpError?.Invoke(this, "Playlist lost!", PlaylistErrorCountInRow);
                                }
                            }

                            updateErrors?.Invoke(this, PlaylistErrorCountInRow, PlaylistErrorCountInRowMax,
                                OtherErrorCountInRow, OtherErrorCountInRowMax, ChunkDownloadErrorCount,
                                ChunkAppendErrorCount, LostChunkCount);

                            if (OtherErrorCountInRow >= OtherErrorCountInRowMax)
                            {
                                dumpError?.Invoke(this, "Max error count limit is reached! Breaking...", OtherErrorCountInRow);
                                break;
                            }

                            if (playlistErrorCode == 200)
                            {
                                if (filteredPlaylist != null && filteredPlaylist.Count > 0)
                                {
                                    for (int i = 0; i < filteredPlaylist.Count; ++i)
                                    {
                                        int tickBeforeChunk = Environment.TickCount;

                                        string chunkUrl = filteredPlaylist[i];
                                        long chunkLength = -1L;
                                        long currentAbsoluteChunkId = CurrentPlaylistFirstNewChunkId + i;

                                        int chunkDownloadErrorCode;
                                        try
                                        {
                                            using (MemoryStream mem = new MemoryStream())
                                            {
                                                FileDownloader d = new FileDownloader() { Url = chunkUrl };
                                                chunkDownloadErrorCode = d.Download(mem);
                                                if (chunkDownloadErrorCode == 200)
                                                {
                                                    chunkLength = mem.Length;
                                                    mem.Position = 0L;
                                                    if (MultiThreadedDownloader.AppendStream(mem, outputStream))
                                                    {
                                                        OtherErrorCountInRow = 0;
                                                        _lastProcessedChunkId = currentAbsoluteChunkId;
                                                        if (writeChunksInfo)
                                                        {
                                                            try
                                                            {
                                                                JObject jChunk = new JObject();
                                                                jChunk["position"] = outputStream.Position - mem.Length;
                                                                jChunk["size"] = mem.Length;
                                                                jChunk["id"] = currentAbsoluteChunkId;
                                                                //TODO: Determine and store other chunk information from playlist
                                                                jChunks.Add(jChunk);
                                                            }
                                                            catch (Exception ex)
                                                            {
                                                                System.Diagnostics.Debug.WriteLine(ex.Message);
                                                                OtherErrorCountInRow++;
                                                                dumpError?.Invoke(this, "Failed to append chunk info", OtherErrorCountInRow);
                                                            }
                                                        }
                                                    }
                                                    else
                                                    {
                                                        ChunkAppendErrorCount++;
                                                        OtherErrorCountInRow++;
                                                        chunkAppendFailed?.Invoke(this, ChunkAppendErrorCount);
                                                        //TODO: The stream and chunks information data will be corrupted here, so it's strongly needed to do some magic thing!
                                                    }
                                                }
                                                else
                                                {
                                                    ChunkDownloadErrorCount++;
                                                    OtherErrorCountInRow++;
                                                    chunkDownloadFailed?.Invoke(this, chunkDownloadErrorCode, ChunkDownloadErrorCount);
                                                }
                                            }
                                        } catch (Exception ex)
                                        {
                                            System.Diagnostics.Debug.WriteLine(ex.Message);
                                            chunkDownloadErrorCode = ex.HResult;
                                            OtherErrorCountInRow++;
                                            dumpError?.Invoke(this, "Failed to append chunk", OtherErrorCountInRow);
                                        }

                                        _chunkUrlList.AddLast(chunkUrl);
                                        if (_chunkUrlList.Count > 50)
                                        {
                                            _chunkUrlList.RemoveFirst();
                                        }

                                        int chunkProcessingTime = Environment.TickCount - tickBeforeChunk;

                                        ProcessedChunkCountTotal++;
                                        nextChunkArrived?.Invoke(this, currentAbsoluteChunkId,
                                            ProcessedChunkCountTotal, chunkLength, chunkProcessingTime, chunkUrl);

                                        dumpProgress?.Invoke(this, outputStream.Length, chunkDownloadErrorCode);

                                        updateErrors?.Invoke(this, PlaylistErrorCountInRow, PlaylistErrorCountInRowMax,
                                            OtherErrorCountInRow, OtherErrorCountInRowMax, ChunkDownloadErrorCount,
                                            ChunkAppendErrorCount, LostChunkCount);

                                        if (_cancellationToken.IsCancellationRequested) { break; }
                                    }
                                }
                            }

                            if (OtherErrorCountInRow >= OtherErrorCountInRowMax)
                            {
                                dumpError?.Invoke(this, "Max error count limit is reached! Breaking...", OtherErrorCountInRow);
                                break;
                            }

                            if (_cancellationToken.IsCancellationRequested) { break; }

                            updateErrors?.Invoke(this, PlaylistErrorCountInRow, PlaylistErrorCountInRowMax,
                               OtherErrorCountInRow, OtherErrorCountInRowMax, ChunkDownloadErrorCount,
                               ChunkAppendErrorCount, LostChunkCount);

                            int elapsedTime = Environment.TickCount - timeStart;
                            LastDelayValueMilliseconds = PlaylistCheckingIntervalMilliseconds - elapsedTime;
                            playlistCheckingDelayCalculated?.Invoke(this,
                                LastDelayValueMilliseconds, PlaylistCheckingIntervalMilliseconds, elapsedTime);
                            if (LastDelayValueMilliseconds > 0)
                            {
                                dumpMessage?.Invoke(this,
                                    $"Waiting for {LastDelayValueMilliseconds} milliseconds " +
                                    $"(max: {PlaylistCheckingIntervalMilliseconds})");
                                Thread.Sleep(LastDelayValueMilliseconds);
                            }
                        } while (OtherErrorCountInRow < OtherErrorCountInRowMax &&
                                PlaylistErrorCountInRow < PlaylistErrorCountInRowMax &&
                                !_cancellationToken.IsCancellationRequested);
                    }
                } catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.Message);
                    OtherErrorCountInRow++;
                    dumpError?.Invoke(this, ex.Message, OtherErrorCountInRow);
                }

                if (writeChunksInfo)
                {
                    try
                    {
                        JObject json = new JObject();
                        json["playlistUrl"] = Url;
                        if (!string.IsNullOrEmpty(ActualUrl) && !string.IsNullOrWhiteSpace(ActualUrl) &&
                            ActualUrl != Url)
                        {
                            json["actualPlaylistUrl"] = ActualUrl;
                        }
                        json["outputFile"] = outputFilePath;
                        json.Add(new JProperty("chunks", jChunks));
                        File.WriteAllText($"{outputFilePath}_chunks.json", json.ToString());
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine(ex.Message);
                        OtherErrorCountInRow++;
                        dumpError?.Invoke(this, ex.Message, OtherErrorCountInRow);
                    }

                    updateErrors?.Invoke(this, PlaylistErrorCountInRow, PlaylistErrorCountInRowMax,
                        OtherErrorCountInRow, OtherErrorCountInRowMax, ChunkDownloadErrorCount,
                        ChunkAppendErrorCount, LostChunkCount);
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
    }
}
