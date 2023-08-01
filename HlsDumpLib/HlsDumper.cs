﻿using System;
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

        private readonly LinkedList<StreamSegment> _chunkList = new LinkedList<StreamSegment>();

        private CancellationTokenSource _cancellationTokenSource;
        private CancellationToken _cancellationToken;

        public const int DUMPING_ERROR_PLAYLIST_GONE = -1;
        public const int DUMPING_ERROR_CANCELED = -2;

        public delegate void PlaylistCheckingDelegate(object sender, string playlistUrl);
        public delegate void PlaylistCheckedDelegate(object sender,
            int chunkCount, int newChunkCount, long firstChunkId, long firstNewChunkId,
            string playlistContent, int errorCode, int playlistErrorCountInRow);
        public delegate void PlaylistFirstArrivedDelegate(object sender, int chunkCount, long firstChunkId);
        public delegate void OutputStreamAssignedDelegate(object sender, Stream stream, string fileName);
        public delegate void OutputStreamClosedDelegate(object sender, string fileName);
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
            OutputStreamAssignedDelegate outputStreamAssigned,
            OutputStreamClosedDelegate outputStreamClosed,
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
                bool headerChunkExists = false;

                JArray jChunks = new JArray();
                FileDownloader playlistDownloader = new FileDownloader() { Url = Url };
                Stream outputStream = null;

                try
                {
                    do
                    {
                        int timeStart = Environment.TickCount;
                        playlistChecking?.Invoke(this, Url);

                        M3UPlaylist playlist = null;
                        List<StreamSegment> unfilteredPlaylist = null;
                        List<StreamSegment> filteredPlaylist = null;
                        int playlistErrorCode = playlistDownloader.DownloadString(out string response);
                        if (playlistErrorCode == 200)
                        {
                            PlaylistErrorCountInRow = 0;

                            playlist = new M3UPlaylist(response, Url);
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
                                    playlist = new M3UPlaylist(response, Url);
                                    playlist.Parse();
                                }

                                headerChunkExists = !string.IsNullOrEmpty(playlist.StreamHeaderSegmentUrl) &&
                                    !string.IsNullOrWhiteSpace(playlist.StreamHeaderSegmentUrl);

                                CurrentSessionFirstChunkId = playlist.MediaSequence >= 0 ? playlist.MediaSequence : 0L;
                                outputFilePath += GetOutputFileExtension(playlist);

                                playlistFirstArrived?.Invoke(this, CurrentPlaylistChunkCount, CurrentSessionFirstChunkId);
                            }

                            _currentPlaylistFirstChunkId = playlist.MediaSequence >= 0 ? playlist.MediaSequence : 0L;

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
                                            OtherErrorCountInRow = 0;
                                            streamHeader.Position = 0L;
                                            if (MultiThreadedDownloader.AppendStream(streamHeader, outputStream))
                                            {
                                                ProcessedChunkCountTotal++;
                                                if (writeChunksInfo)
                                                {
                                                    try
                                                    {
                                                        JObject jChunk = new JObject();
                                                        jChunk["position"] = outputStream.Position - streamHeader.Length;
                                                        jChunk["size"] = streamHeader.Length;
                                                        jChunk["id"] = 0;
                                                        jChunks.Add(jChunk);
                                                    }
                                                    catch (Exception ex)
                                                    {
                                                        System.Diagnostics.Debug.WriteLine(ex.Message);
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
                                    long chunkLength = -1L;
                                    long currentAbsoluteChunkId = CurrentPlaylistFirstNewChunkId + i;

                                    int chunkDownloadErrorCode;
                                    try
                                    {
                                        using (MemoryStream mem = new MemoryStream())
                                        {
                                            FileDownloader d = new FileDownloader() { Url = chunk.Url };
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

                                    _chunkList.AddLast(chunk);
                                    if (_chunkList.Count > 50)
                                    {
                                        _chunkList.RemoveFirst();
                                    }

                                    int chunkProcessingTime = Environment.TickCount - tickBeforeChunk;

                                    ProcessedChunkCountTotal++;
                                    nextChunkArrived?.Invoke(this, currentAbsoluteChunkId,
                                        ProcessedChunkCountTotal, chunkLength, chunkProcessingTime, chunk.Url);

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
