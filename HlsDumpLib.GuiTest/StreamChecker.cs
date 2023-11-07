using System;
using System.Threading.Tasks;
using MultiThreadedDownloaderLib;
using static HlsDumpLib.HlsDumper;

namespace HlsDumpLib.GuiTest
{
    internal class StreamChecker
    {
        public StreamItem StreamItem { get; set; }

        public delegate void CheckingStartedDelegate(object sender);
        public delegate void CheckingFinishedDelegate(object sender, int errorCode);
        public delegate void DumpingStartedDelegate(object sender);

        public void Check(string outputFilePath,
            CheckingStartedDelegate checkingStarted,
            CheckingFinishedDelegate checkingFinished,
            PlaylistCheckingStartedDelegate playlistCheckingStarted,
            PlaylistCheckingFinishedDelegate playlistCheckingFinished,
            PlaylistFirstArrivedDelegate playlistFirstArrived,
            OutputStreamAssignedDelegate outputStreamAssigned,
            OutputStreamClosedDelegate outputStreamClosed,
            PlaylistCheckingDelayCalculatedDelegate playlistCheckingDelayCalculated,
            DumpingStartedDelegate dumpingStarted,
            NextChunkArrivedDelegate nextChunkArrived,
            ErrorsUpdatedDelegate errorsUpdated,
            DumpProgressDelegate dumpingProgress,
            DumpFinishedDelegate dumpingFinished,
            int playlistCheckingIntervalMilliseconds,
            int maxPlaylistErrorCountInRow,
            int maxOtherErrorsInRow,
            bool saveChunksInfo,
            bool storeChunkFileName,
            bool storeChunkUrl,
            bool useGmtTime)
        {
            checkingStarted?.Invoke(this);

            int errorCode = FileDownloader.GetUrlContentLength(StreamItem.PlaylistUrl, null, out _, out _);
            if (errorCode == 200)
            {
                if (!StreamItem.IsDumping)
                {
                    StreamItem.DumpStarted = DateTime.Now;
                    StreamItem.Dumper = new HlsDumper(StreamItem.PlaylistUrl);
                    dumpingStarted?.Invoke(this);
                    Task.Run(() => StreamItem.Dumper.Dump(outputFilePath,
                        (s, url) => { playlistCheckingStarted?.Invoke(this, url); },
                        (s, chunkCount, newChunkCount, firstChunkId, firstNewChunkId, playlistContent, e, playlistErrorCountInRow) =>
                            { playlistCheckingFinished?.Invoke(this, chunkCount, newChunkCount, firstChunkId, firstNewChunkId, playlistContent, e, playlistErrorCountInRow); },
                        (s, count, first) => { playlistFirstArrived?.Invoke(this, count, first); },
                        (s, stream, fn) => { outputStreamAssigned?.Invoke(this, stream, fn); },
                        (s, fn) => { outputStreamClosed?.Invoke(this, fn); },
                        (s, delay, checkingInterval, cycleProcessingTime) =>
                            { playlistCheckingDelayCalculated?.Invoke(this, delay, checkingInterval, cycleProcessingTime); },
                        (s, chunk, chunkSize, sessionChunkId, chunkProcessingTime) =>
                            { nextChunkArrived?.Invoke(this, chunk, chunkSize, sessionChunkId, chunkProcessingTime); },
                        (s, playlistErrorCountInRow, playlistErrorCountInRowMax,
                        otherErrorCountInRow, otherErrorCountInRowMax,
                        chunkDownloadErrorCount, chunkAppendErrorCount, lostChunkCount) =>
                            {
                                errorsUpdated?.Invoke(this, playlistErrorCountInRow, playlistErrorCountInRowMax,
                                otherErrorCountInRow, otherErrorCountInRowMax,
                                chunkDownloadErrorCount, chunkAppendErrorCount, lostChunkCount);
                            },
                        (s, fs, e) => { dumpingProgress.Invoke(this, fs, e); }, null,
                        null, null, null, null, (s, e) =>
                        {
                            dumpingFinished.Invoke(this, e);
                            StreamItem.Dumper = null;
                        },
                        playlistCheckingIntervalMilliseconds,
                        maxPlaylistErrorCountInRow, maxOtherErrorsInRow,
                        saveChunksInfo, storeChunkFileName, storeChunkUrl, useGmtTime));
                }
            }

            checkingFinished?.Invoke(this, errorCode);
        }
    }
}
