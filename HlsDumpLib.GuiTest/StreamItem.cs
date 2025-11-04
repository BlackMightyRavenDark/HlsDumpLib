using System;
using static HlsDumpLib.HlsDumper;

namespace HlsDumpLib.GuiTest
{
	internal class StreamItem
	{
		public string Title { get; }
		public string PlaylistUrl { get; }
		public string OutputFilePath { get; }
		public DateTime DumpStarted { get; private set; } = DateTime.MaxValue;
		public HlsDumper Dumper { get; private set; }
		public bool IsChecking { get; private set; }
		public bool IsDumping => Dumper != null;
		public bool IsRemoving { get; set; }

		public delegate void CheckStartedDelegate(object sender);
		public delegate void CheckFinishedDelegate(object sender, int errorCode);
		public delegate void DumpStartedDelegate(object sender);

		public StreamItem(string title, string playlistUrl, string outputFilePath)
		{
			Title = title;
			PlaylistUrl = playlistUrl;
			OutputFilePath = outputFilePath;
		}

		public void Check(
			CheckStartedDelegate checkStarted,
			CheckFinishedDelegate checkFinished,
			PlaylistCheckStartedDelegate playlistCheckStarted,
			PlaylistCheckFinishedDelegate playlistCheckFinished,
			PlaylistFirstArrivedDelegate playlistFirstArrived,
			OutputStreamAssignedDelegate outputStreamAssigned,
			OutputStreamClosedDelegate outputStreamClosed,
			PlaylistCheckDelayCalculatedDelegate playlistCheckDelayCalculated,
			DumpStartedDelegate dumpStarted,
			NextChunkConnectingDelegate nextChunkConnecting,
			NextChunkConnectedDelegate nextChunkConnected,
			NextChunkProcessedDelegate nextChunkProcessed,
			ErrorsUpdatedDelegate errorsUpdated,
			DumpProgressDelegate dumpProgress,
			DumpFinishedDelegate dumpFinished,
			int playlistCheckIntervalMilliseconds,
			int maxPlaylistErrorCountInRow,
			int maxOtherErrorsInRow,
			int connectionTimeout,
			bool saveChunksInfo,
			bool storeChunkFileName,
			bool storeChunkUrl,
			bool useGmtTime)
		{
			if (!IsChecking)
			{
				IsChecking = true;

				checkStarted?.Invoke(this);

				int errorCode = MultiThreadedDownloaderLib.Utils.GetUrlResponseHeaders(PlaylistUrl, null, out _, out _);
				if (errorCode == 200)
				{
					if (!IsDumping)
					{
						DumpStarted = DateTime.UtcNow;
						Dumper = new HlsDumper(PlaylistUrl);
						dumpStarted?.Invoke(this);

						Dumper.Dump(OutputFilePath,
							(s, url) => playlistCheckStarted?.Invoke(this, url),
							(s, chunkCount, newChunkCount, firstChunkId, firstNewChunkId, playlistContent, e, playlistErrorCountInRow) =>
								playlistCheckFinished?.Invoke(this, chunkCount, newChunkCount, firstChunkId, firstNewChunkId, playlistContent, e, playlistErrorCountInRow),
							(s, count, first, manifestItem) => playlistFirstArrived?.Invoke(this, count, first, manifestItem),
							(s, stream, fn) => outputStreamAssigned?.Invoke(this, stream, fn),
							(s, fn) => outputStreamClosed?.Invoke(this, fn),
							(s, delay, checkInterval, cycleProcessingTime) =>
								playlistCheckDelayCalculated?.Invoke(this, delay, checkInterval, cycleProcessingTime),
							(s, chunk) => nextChunkConnecting?.Invoke(this, chunk),
							(s, chunk, chunkSize, code) => nextChunkConnected?.Invoke(this, chunk, chunkSize, code),
							(s, chunk, chunkSize, sessionChunkId, chunkProcessingTime) =>
								nextChunkProcessed?.Invoke(this, chunk, chunkSize, sessionChunkId, chunkProcessingTime),
							(s, playlistErrorCountInRow, playlistErrorCountInRowMax,
							otherErrorCountInRow, otherErrorCountInRowMax,
							chunkDownloadErrorCount, chunkAppendErrorCount, lostChunkCount) =>
								errorsUpdated?.Invoke(this, playlistErrorCountInRow, playlistErrorCountInRowMax,
								otherErrorCountInRow, otherErrorCountInRowMax,
								chunkDownloadErrorCount, chunkAppendErrorCount, lostChunkCount),
							(s, fs, e) => dumpProgress?.Invoke(this, fs, e),
							null, null, null, null, null,
							(s, e, t) =>
							{
								dumpFinished?.Invoke(this, e, t);
								Dumper = null;
							},
							playlistCheckIntervalMilliseconds,
							maxPlaylistErrorCountInRow, maxOtherErrorsInRow, connectionTimeout,
							saveChunksInfo, storeChunkFileName, storeChunkUrl, useGmtTime);
					}
				}

				if (IsDumping && checkFinished != null)
				{
					checkFinished.Invoke(this, errorCode);
				}

				IsChecking = false;
			}
		}
	}
}
