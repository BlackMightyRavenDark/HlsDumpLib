﻿using System;
using System.Threading.Tasks;
using MultiThreadedDownloaderLib;

namespace HlsDumpLib.GuiTest
{
    internal class StreamChecker
    {
        public StreamItem StreamItem { get; set; }

        public delegate void CheckingStartedDelegate(object sender);
        public delegate void CheckingFinishedDelegate(object sender, int errorCode);
        public delegate void DumpingStartedDelegate(object sender);
        public delegate void DumpingProgressDelegate(object sender, long fileSize, int errorCode);
        public delegate void DumpingFinishedDelegate(object sender, int errorCode);

        public void Check(string filePath,
            CheckingStartedDelegate checkingStarted,
            CheckingFinishedDelegate checkingFinished,
            DumpingStartedDelegate dumpingStarted,
            DumpingProgressDelegate dumpingProgress,
            DumpingFinishedDelegate dumpingFinished)
        {
            checkingStarted?.Invoke(this);

            int errorCode = FileDownloader.GetUrlContentLength(StreamItem.PlaylistUrl, out _, out _);
            if (errorCode == 200)
            {
                if (!StreamItem.IsLive)
                {
                    StreamItem.DumpStarted = DateTime.Now;
                    StreamItem.Dumper = new HlsDumper(StreamItem.PlaylistUrl);
                    dumpingStarted?.Invoke(this);
                    Task.Run(() => StreamItem.Dumper.Dump(filePath, null, null, (s, fs, e) => { dumpingProgress.Invoke(this, fs, e); }, null,
                        null, null, null, null, (s, e) =>
                        {
                            dumpingFinished.Invoke(this, e);
                            StreamItem.Dumper = null;
                        }, true));
                }
            }

            checkingFinished?.Invoke(this, errorCode);
        }
    }
}