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
        public delegate void PlaylistCheckingFinishedDelegate(object sender, int errorCode);
        public delegate void DumpingStartedDelegate(object sender);

        public void Check(string outputFilePath,
            CheckingStartedDelegate checkingStarted,
            CheckingFinishedDelegate checkingFinished,
            PlaylistCheckingStartedDelegate playlistCheckingStarted,
            PlaylistCheckingFinishedDelegate playlistCheckingFinished,
            HlsDumper.PlaylistFirstArrived playlistFirstArrived,
            DumpingStartedDelegate dumpingStarted,
            HlsDumper.NextChunkDelegate nextChunk,
            HlsDumper.DumpProgressDelegate dumpingProgress,
            HlsDumper.DumpFinishedDelegate dumpingFinished,
            bool saveChunksInfo)
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
                        (s, e) => { playlistCheckingFinished?.Invoke(this, e); },
                        (s, count, first) => { playlistFirstArrived?.Invoke(this, count, first); },
                        (s, absoluteChunkId, sessionChunkId,chunkSize, chunkProcessingTime, chunkUrl) =>
                            { nextChunk?.Invoke(this, absoluteChunkId, sessionChunkId, chunkSize, chunkProcessingTime, chunkUrl); },
                        (s, fs, e) => { dumpingProgress.Invoke(this, fs, e); }, null,
                        null, null, null, null, (s, e) =>
                        {
                            dumpingFinished.Invoke(this, e);
                            StreamItem.Dumper = null;
                        }, saveChunksInfo, true));
                }
            }

            checkingFinished?.Invoke(this, errorCode);
        }
    }
}
