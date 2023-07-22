using System;
using System.Threading.Tasks;
using MultiThreadedDownloaderLib;

namespace HlsDumpLib.GuiTest
{
    internal class StreamChecker
    {
        public StreamItem StreamItem { get; set; }

        public delegate void CheckingStartedDelegate(object sender);
        public delegate void CheckingFinishedDelegate(object sender, int errorCode);
        public delegate void PlaylistCheckingStartedDelegate(object sender, string url);
        public delegate void PlaylistCheckingFinishedDelegate(object sender,
            int chunkCount, int newChunkCount, long firstChunkId, long firstNewChunkId,
            string playlistContent, int errorCode, int playlistErrorCountInRow);
        public delegate void DumpingStartedDelegate(object sender);

        public void Check(string outputFilePath,
            CheckingStartedDelegate checkingStarted,
            CheckingFinishedDelegate checkingFinished,
            PlaylistCheckingStartedDelegate playlistCheckingStarted,
            PlaylistCheckingFinishedDelegate playlistCheckingFinished,
            HlsDumper.PlaylistFirstArrivedDelegate playlistFirstArrived,
            DumpingStartedDelegate dumpingStarted,
            HlsDumper.NextChunkArrivedDelegate nextChunkArrived,
            HlsDumper.UpdateErrorsDelegate updateErrors,
            HlsDumper.DumpProgressDelegate dumpingProgress,
            HlsDumper.DumpFinishedDelegate dumpingFinished,
            int playlistCheckingIntervalMilliseconds,
            bool saveChunksInfo,
            int maxPlaylistErrorCountInRow,
            int maxOtherErrorsInRow)
        {
            checkingStarted?.Invoke(this);

            int errorCode = FileDownloader.GetUrlContentLength(StreamItem.PlaylistUrl, out _, out _);
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
                        (s, absoluteChunkId, sessionChunkId,chunkSize, chunkProcessingTime, chunkUrl) =>
                            { nextChunkArrived?.Invoke(this, absoluteChunkId, sessionChunkId, chunkSize, chunkProcessingTime, chunkUrl); },
                        (s, playlistErrorCountInRow, playlistErrorCountInRowMax,
                        otherErrorCountInRow, otherErrorCountInRowMax,
                        chunkDownloadErrorCount, chunkAppendErrorCount, lostChunkCount) =>
                            {
                                updateErrors?.Invoke(this, playlistErrorCountInRow, playlistErrorCountInRowMax,
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
                        saveChunksInfo, maxPlaylistErrorCountInRow, maxOtherErrorsInRow));
                }
            }

            checkingFinished?.Invoke(this, errorCode);
        }
    }
}
