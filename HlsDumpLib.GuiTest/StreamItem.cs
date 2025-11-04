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

				int playlistErrorCode = MultiThreadedDownloaderLib.Utils.GetUrlResponseHeaders(PlaylistUrl, null, out _, out _);
				if (playlistErrorCode == 200)
				{
					if (!IsDumping)
					{
						DumpStarted = DateTime.UtcNow;
						Dumper = new HlsDumper(PlaylistUrl);
						dumpStarted?.Invoke(this);

						HlsDumperParameters dumperParameters = new HlsDumperParameters()
						{
							OutputFilePath = OutputFilePath,
							PlaylistCheckIntervalMilliseconds = playlistCheckIntervalMilliseconds,
							MaxPlaylistErrorsInRow = maxPlaylistErrorCountInRow,
							MaxOtherErrorsInRow = maxOtherErrorsInRow,
							ConnectionTimeoutMilliseconds = connectionTimeout,
							WriteChunkInfo = saveChunksInfo,
							StoreChunkFileName = storeChunkFileName,
							StoreChunkUrl = storeChunkUrl,
							UseGmtTime = useGmtTime
						};
						if (playlistCheckStarted != null)
						{
							dumperParameters.PlaylistCheckStarted += (s, url) => playlistCheckStarted.Invoke(this, url);
						}
						if (playlistCheckFinished != null)
						{
							dumperParameters.PlaylistCheckFinished +=
								(s, chunkCount, newChunkCount, firstChunkId, firstNewChunkId, playlistContent, errorCode, playlistErrorCountInRow) =>
									playlistCheckFinished.Invoke(this, chunkCount, newChunkCount, firstChunkId, firstNewChunkId, playlistContent, errorCode, playlistErrorCountInRow);
						}
						if (playlistFirstArrived != null)
						{
							dumperParameters.PlaylistFirstArrived += (s, chunkCount, firstChunkId, manifestItem) =>
								playlistFirstArrived.Invoke(this, chunkCount, firstChunkId, manifestItem);
						}
						if (outputStreamAssigned != null)
						{
							dumperParameters.OutputStreamAssigned += (s, stream, filePath) => outputStreamAssigned.Invoke(this, stream, filePath);
						}
						if (outputStreamClosed != null)
						{
							dumperParameters.OutputStreamClosed += (s, filePath) => outputStreamClosed.Invoke(this, filePath);
						}
						if (playlistCheckDelayCalculated != null)
						{
							dumperParameters.PlaylistCheckDelayCalculated += (s, delay, checkInterval, cycleProcessingTime) =>
								playlistCheckDelayCalculated.Invoke(this, delay, checkInterval, cycleProcessingTime);
						}
						if (nextChunkConnecting != null)
						{
							dumperParameters.NextChunkConnecting += (s, chunk) => nextChunkConnecting.Invoke(this, chunk);
						}
						if (nextChunkConnected != null)
						{
							dumperParameters.NextChunkConnected += (s, chunk, chunkSize, errorCode) =>
								nextChunkConnected.Invoke(this, chunk, chunkSize, errorCode);
						}
						if (nextChunkProcessed != null)
						{
							dumperParameters.NextChunkProcessed += (s, chunk, chunkSize, sessionChunkId, chunkProcessingTime) =>
								nextChunkProcessed.Invoke(this, chunk, chunkSize, sessionChunkId, chunkProcessingTime);
						}
						if (errorsUpdated != null)
						{
							dumperParameters.ErrorsUpdated +=
								(s, playlistErrorCountInRow, playlistErrorCountInRowMax,
								otherErrorCountInRow, otherErrorCountInRowMax,
								chunkDownloadErrorCount, chunkAppendErrorCount, lostChunkCount) =>
									errorsUpdated.Invoke(this, playlistErrorCountInRow, playlistErrorCountInRowMax,
									otherErrorCountInRow, otherErrorCountInRowMax,
									chunkDownloadErrorCount, chunkAppendErrorCount, lostChunkCount);
						}
						if (dumpProgress != null)
						{
							dumperParameters.DumpProgress += (s, outputFileSize, errorCode) => dumpProgress.Invoke(this, outputFileSize, errorCode);
						}
						dumperParameters.DumpFinished += (s, errorCode, errorMessage) =>
						{
							dumpFinished?.Invoke(this, errorCode, errorMessage);
							Dumper = null;
						};

						Dumper.Dump(dumperParameters);
					}
				}

				if (IsDumping && checkFinished != null)
				{
					checkFinished.Invoke(this, playlistErrorCode);
				}

				IsChecking = false;
			}
		}
	}
}
