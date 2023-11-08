using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using MultiThreadedDownloaderLib;

namespace HlsDumpLib
{
    public class HlsDumper : IDisposable
    {
        public string Url { get; }
        public string ActualUrl { get; set; }
        public int ProcessedChunkCountTotal { get; private set; } = 0;
        public int ChunkDownloadErrorCount { get; private set; } = 0;
        public int ChunkAppendErrorCount { get; private set; } = 0;

        public int CurrentSessionFirstChunkId { get; private set; } = 0;
        public int CurrentPlaylistFirstNewChunkId { get; private set; } = 0;
        public int CurrentPlaylistChunkCount { get; private set; } = 0;
        public int CurrentPlaylistNewChunkCount { get; private set; } = 0;
        public int LostChunkCount { get; private set; } = 0;
        public int LastDelayValueMilliseconds { get; private set; } = 0;
        public int PlaylistErrorCountInRowMax { get; private set; } = 5;
        public int PlaylistErrorCountInRow { get; private set; } = 0;
        public int OtherErrorCountInRowMax { get; private set; } = 5;
        public int OtherErrorCountInRow { get; private set; } = 0;

        public int PlaylistCheckingIntervalMilliseconds { get; private set; } = 2000;

        private int _currentPlaylistFirstChunkId = -1;
        private int _lastProcessedChunkId = -1;

        private readonly LinkedList<StreamSegment> _chunkList = new LinkedList<StreamSegment>();

        private CancellationTokenSource _cancellationTokenSource;
        private CancellationToken _cancellationToken;

        public const int DUMPING_ERROR_PLAYLIST_GONE = -1;
        public const int DUMPING_ERROR_CANCELED = -2;

        public delegate void PlaylistCheckingStartedDelegate(object sender, string playlistUrl);
        public delegate void PlaylistCheckingFinishedDelegate(object sender,
            int chunkCount, int newChunkCount, int firstChunkId, int firstNewChunkId,
            string playlistContent, int errorCode, int playlistErrorCountInRow);
        public delegate void PlaylistFirstArrivedDelegate(object sender, int chunkCount, int firstChunkId);
        public delegate void OutputStreamAssignedDelegate(object sender, Stream stream, string fileName);
        public delegate void OutputStreamClosedDelegate(object sender, string fileName);
        public delegate void PlaylistCheckingDelayCalculatedDelegate(object sender,
            int delay, int checkingInterval, int cycleProcessingTime);
        public delegate void NextChunkConnectingDelegate(object sender, StreamSegment chunk);
        public delegate void NextChunkConnectedDelegate(object sender, StreamSegment chunk, long chunkFileSize, int errorCode);
        public delegate void NextChunkProcessedDelegate(object sender, StreamSegment chunk,
            long chunkSize, int sessionChunkId, int chunkProcessingTime);
        public delegate void ErrorsUpdatedDelegate(object sender,
            int playlistErrorCountInRow, int playlistErrorCountInRowMax,
            int otherErrorCountInRow, int otherErrorCountInRowMax,
            int chunkDownloadErrorCount, int chunkAppendErrorCount,
            int lostChunkCount);
        public delegate void DumpProgressDelegate(object sender, long fileSize, int errorCode);
        public delegate void ChunkDownloadFailedDelegate(object sender, int errorCode, int failedCount);
        public delegate void ChunkAppendFailedDelegate(object sender, int failedCount);
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
            PlaylistCheckingStartedDelegate playlistCheckingStarted,
            PlaylistCheckingFinishedDelegate playlistCheckingFinished,
            PlaylistFirstArrivedDelegate playlistFirstArrived,
            OutputStreamAssignedDelegate outputStreamAssigned,
            OutputStreamClosedDelegate outputStreamClosed,
            PlaylistCheckingDelayCalculatedDelegate playlistCheckingDelayCalculated,
            NextChunkConnectingDelegate nextChunkConnecting,
            NextChunkConnectedDelegate nextChunkConnected,
            NextChunkProcessedDelegate nextChunkProcessed,
            ErrorsUpdatedDelegate errorsUpdated,
            DumpProgressDelegate dumpProgress,
            ChunkDownloadFailedDelegate chunkDownloadFailed,
            ChunkAppendFailedDelegate chunkAppendFailed,
            DumpMessageDelegate dumpMessage,
            DumpWarningDelegate dumpWarning,
            DumpErrorDelegate dumpError,
            DumpFinishedDelegate dumpFinished,
            int playlistCheckingIntervalMilliseconds,
            int maxPlaylistErrorCountInRow,
            int maxOtherErrorsInRow,
            bool writeChunksInfo,
            bool storeChunkFileName,
            bool storeChunkUrl,
            bool useGmtTime)
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
                bool headerChunkExists = false;

                JObject jHeaderChunk = null;
                JArray jaValidChunks = new JArray();
                JArray jaLostChunks = new JArray();
                FileDownloader playlistDownloader = new FileDownloader() { Url = Url };
                Stream outputStream = null;

                try
                {
                    do
                    {
                        int timeStart = Environment.TickCount;
                        playlistCheckingStarted?.Invoke(this, Url);

                        M3UPlaylist playlist = null;
                        List<StreamSegment> unfilteredPlaylist = null;
                        List<StreamSegment> filteredPlaylist = null;
                        int playlistErrorCode = playlistDownloader.DownloadString(out string response);
                        if (playlistErrorCode == 200)
                        {
                            PlaylistErrorCountInRow = 0;

                            playlist = new M3UPlaylist(response, Url, useGmtTime);
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
                                    playlist = new M3UPlaylist(response, Url, useGmtTime);
                                    playlist.Parse();
                                }

                                headerChunkExists = !string.IsNullOrEmpty(playlist.StreamHeaderSegmentUrl) &&
                                    !string.IsNullOrWhiteSpace(playlist.StreamHeaderSegmentUrl);

                                CurrentSessionFirstChunkId = playlist.MediaSequence >= 0 ? playlist.MediaSequence : 0;
                                outputFilePath += GetOutputFileExtension(playlist);

                                playlistFirstArrived?.Invoke(this, CurrentPlaylistChunkCount, CurrentSessionFirstChunkId);
                            }

                            _currentPlaylistFirstChunkId = playlist.MediaSequence >= 0 ? playlist.MediaSequence : 0;

                            unfilteredPlaylist = new List<StreamSegment>();
                            if (playlist.Segments != null)
                            {
                                unfilteredPlaylist.AddRange(playlist.Segments);
                            }
                            CurrentPlaylistChunkCount = unfilteredPlaylist.Count;

                            filteredPlaylist = playlist.Filter(_chunkList)?.ToList();
                            if (filteredPlaylist != null)
                            {
                                CurrentPlaylistNewChunkCount = filteredPlaylist.Count;
                                CurrentPlaylistFirstNewChunkId = _currentPlaylistFirstChunkId +
                                    CurrentPlaylistChunkCount - CurrentPlaylistNewChunkCount;
                            }
                            else
                            {
                                CurrentPlaylistNewChunkCount = 0;
                                CurrentPlaylistFirstNewChunkId = -1;
                            }

                            int diff = _lastProcessedChunkId >= 0 ? CurrentPlaylistFirstNewChunkId - _lastProcessedChunkId : 1;
                            int lost = diff - 1;
                            if (lost > 0)
                            {
                                LostChunkCount += lost;
                                OtherErrorCountInRow++;
                                for (int i = _lastProcessedChunkId + 1; i < CurrentPlaylistFirstNewChunkId; ++i)
                                {
                                    jaLostChunks.Add(i);
                                }
                                dumpError?.Invoke(this, $"Lost: {lost}, Total lost: {LostChunkCount})", -1);
                            }

                            playlistCheckingFinished?.Invoke(this,
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

                            playlistCheckingFinished?.Invoke(this,
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
                                    errorsUpdated?.Invoke(this, PlaylistErrorCountInRow, PlaylistErrorCountInRowMax,
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

                        errorsUpdated?.Invoke(this, PlaylistErrorCountInRow, PlaylistErrorCountInRowMax,
                            OtherErrorCountInRow, OtherErrorCountInRowMax, ChunkDownloadErrorCount,
                            ChunkAppendErrorCount, LostChunkCount);

                        if (OtherErrorCountInRow >= OtherErrorCountInRowMax)
                        {
                            dumpError?.Invoke(this, "Max error count limit is reached! Breaking...", OtherErrorCountInRow);
                            break;
                        }

                        if (outputStream == null)
                        {
                            outputFilePath = MultiThreadedDownloader.GetNumberedFileName(outputFilePath);
                            outputStream = File.OpenWrite(outputFilePath);
                            outputStreamAssigned?.Invoke(this, outputStream, outputFilePath);
                        }

                        if (playlistErrorCode == 200)
                        {
                            if (headerChunkExists)
                            {
                                headerChunkExists = false;
                                try
                                {
                                    using (MemoryStream streamHeader = new MemoryStream())
                                    {
                                        FileDownloader d = new FileDownloader() { Url = playlist?.StreamHeaderSegmentUrl };
                                        int headerErrorCode = d.Download(streamHeader);
                                        if (headerErrorCode == 200)
                                        {
                                            string chunkHeaderFileName = Utils.ExtractUrlFileName(playlist.StreamHeaderSegmentUrl);
                                            StreamSegment headerChunk = new StreamSegment(DateTime.MinValue,
                                                0.0, -1, chunkHeaderFileName, playlist.StreamHeaderSegmentUrl);
                                            OtherErrorCountInRow = 0;
                                            streamHeader.Position = 0L;
                                            if (StreamAppender.Append(streamHeader, outputStream))
                                            {
                                                ProcessedChunkCountTotal++;
                                                if (writeChunksInfo)
                                                {
                                                    try
                                                    {
                                                        long size = streamHeader.Length;
                                                        long position = outputStream.Position - size;
                                                        jHeaderChunk = headerChunk.ToJson(position, size, true, true);
                                                    }
                                                    catch (Exception ex)
                                                    {
                                                        System.Diagnostics.Debug.WriteLine(ex.Message);
                                                        jHeaderChunk = null;
                                                        OtherErrorCountInRow++;
                                                        dumpError?.Invoke(this, "Failed to append header (metadata) chunk info", OtherErrorCountInRow);
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                OtherErrorCountInRow++;
                                                dumpError?.Invoke(this,
                                                    "Header (metadata) chunk append error! Video might be unplayable!",
                                                    OtherErrorCountInRow);
                                            }
                                        }
                                        else
                                        {
                                            OtherErrorCountInRow++;
                                            dumpError?.Invoke(this,
                                                "Header (metadata) chunk download error! Video will be unplayable!",
                                                OtherErrorCountInRow);
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    System.Diagnostics.Debug.WriteLine(ex.Message);
                                    jHeaderChunk = null;
                                    ChunkDownloadErrorCount++;
                                    OtherErrorCountInRow++;
                                    dumpError?.Invoke(this,
                                        "Header (metadata) chunk processing error! Video might be unplayable!",
                                        OtherErrorCountInRow);
                                }
                            }

                            if (filteredPlaylist != null && filteredPlaylist.Count > 0)
                            {
                                for (int i = 0; i < filteredPlaylist.Count; ++i)
                                {
                                    int tickBeforeChunk = Environment.TickCount;

                                    StreamSegment chunk = filteredPlaylist[i];
                                    long chunkFileSize = -1L;

                                    int chunkDownloadErrorCode;
                                    try
                                    {
                                        using (MemoryStream mem = new MemoryStream())
                                        {
                                            FileDownloader d = new FileDownloader() { Url = chunk.Url };
                                            d.Connecting += (s, url) =>
                                            {
                                                nextChunkConnecting?.Invoke(this, chunk);
                                            };
                                            d.Connected += (s, url, chunkSize, code) =>
                                            {
                                                nextChunkConnected?.Invoke(this, chunk, chunkSize, code);
                                                return code;
                                            };

                                            chunkDownloadErrorCode = d.Download(mem);
                                            if (chunkDownloadErrorCode == 200)
                                            {
                                                chunkFileSize = mem.Length;
                                                mem.Position = 0L;
                                                if (StreamAppender.Append(mem, outputStream))
                                                {
                                                    OtherErrorCountInRow = 0;
                                                    _lastProcessedChunkId = chunk.Id;
                                                    if (writeChunksInfo)
                                                    {
                                                        try
                                                        {
                                                            long size = mem.Length;
                                                            long position = outputStream.Position - size;
                                                            JObject jChunk = chunk.ToJson(position, size, storeChunkFileName, storeChunkUrl);
                                                            jaValidChunks.Add(jChunk);
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
                                                jaLostChunks.Add(chunk.Id);
                                                chunkDownloadFailed?.Invoke(this, chunkDownloadErrorCode, ChunkDownloadErrorCount);
                                            }
                                        }
                                    } catch (Exception ex)
                                    {
                                        System.Diagnostics.Debug.WriteLine(ex.Message);
                                        chunkDownloadErrorCode = ex.HResult;
                                        OtherErrorCountInRow++;
                                        jaLostChunks.Add(chunk.Id);
                                        dumpError?.Invoke(this, "Failed to append chunk", OtherErrorCountInRow);
                                    }

                                    _chunkList.AddLast(chunk);
                                    if (_chunkList.Count > 50)
                                    {
                                        _chunkList.RemoveFirst();
                                    }

                                    int chunkProcessingTime = Environment.TickCount - tickBeforeChunk;

                                    nextChunkProcessed?.Invoke(this, chunk, chunkFileSize,
                                        ProcessedChunkCountTotal, chunkProcessingTime);
                                    ProcessedChunkCountTotal++;

                                    dumpProgress?.Invoke(this, outputStream.Length, chunkDownloadErrorCode);

                                    errorsUpdated?.Invoke(this, PlaylistErrorCountInRow, PlaylistErrorCountInRowMax,
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

                        errorsUpdated?.Invoke(this, PlaylistErrorCountInRow, PlaylistErrorCountInRowMax,
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
                } catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.Message);
                    OtherErrorCountInRow++;
                    dumpError?.Invoke(this, ex.Message, OtherErrorCountInRow);
                }

                if (outputStream != null)
                {
                    outputStream.Close();
                    outputStream = null;
                    outputStreamClosed?.Invoke(this, outputFilePath);
                }

                errorsUpdated?.Invoke(this, PlaylistErrorCountInRow, PlaylistErrorCountInRowMax,
                    OtherErrorCountInRow, OtherErrorCountInRowMax, ChunkDownloadErrorCount,
                    ChunkAppendErrorCount, LostChunkCount);

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
                        if (jHeaderChunk != null)
                        {
                            json.Add(new JProperty("headerChunk", jHeaderChunk));
                        }
                        if (jaLostChunks.Count > 0)
                        {
                            json["lostChunkCount"] = jaLostChunks.Count;
                            json.Add(new JProperty("lostChunks", jaLostChunks));
                        }
                        json["chunkCount"] = jaValidChunks.Count;
                        json.Add(new JProperty("chunks", jaValidChunks));
                        File.WriteAllText($"{outputFilePath}_chunks.json", json.ToString());
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine(ex.Message);
                        OtherErrorCountInRow++;
                        dumpError?.Invoke(this, ex.Message, OtherErrorCountInRow);
                    }
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

        private string GetOutputFileExtension(M3UPlaylist playlist)
        {
            if (playlist.Segments == null || playlist.Segments.Count == 0)
            {
                return ".ts";
            }

            string url = playlist.Segments[0].Url;
            if (!string.IsNullOrEmpty(url) && !string.IsNullOrWhiteSpace(url))
            {
                string ext = Path.GetExtension(url);
                return string.IsNullOrEmpty(ext) || string.IsNullOrWhiteSpace(ext) ? ".ts" : ext;
            }

            return ".ts";
        }
    }
}
