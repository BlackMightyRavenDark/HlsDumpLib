using System;
using System.Threading.Tasks;
using MultiThreadedDownloaderLib;
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

		public delegate void CheckingStartedDelegate(object sender);
		public delegate void CheckingFinishedDelegate(object sender, int errorCode);
		public delegate void DumpingStartedDelegate(object sender);

		public StreamItem(string title, string playlistUrl, string outputFilePath)
		{
			Title = title;
			PlaylistUrl = playlistUrl;
			OutputFilePath = outputFilePath;
		}

		public async void Check(
			CheckingStartedDelegate checkingStarted,
			CheckingFinishedDelegate checkingFinished,
			PlaylistCheckingStartedDelegate playlistCheckingStarted,
			PlaylistCheckingFinishedDelegate playlistCheckingFinished,
			PlaylistFirstArrivedDelegate playlistFirstArrived,
			OutputStreamAssignedDelegate outputStreamAssigned,
			OutputStreamClosedDelegate outputStreamClosed,
			PlaylistCheckingDelayCalculatedDelegate playlistCheckingDelayCalculated,
			DumpingStartedDelegate dumpingStarted,
			NextChunkConnectingDelegate nextChunkConnecting,
			NextChunkConnectedDelegate nextChunkConnected,
			NextChunkProcessedDelegate nextChunkProcessed,
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
			if (!IsChecking)
			{
				IsChecking = true;

				await Task.Run(() =>
				{
					checkingStarted?.Invoke(this);

					int errorCode = FileDownloader.GetUrlResponseHeaders(PlaylistUrl, null, out _, out _);
					if (errorCode == 200)
					{
						if (!IsDumping)
						{
							DumpStarted = DateTime.UtcNow;
							Dumper = new HlsDumper(PlaylistUrl);
							dumpingStarted?.Invoke(this);

							Task.Run(() => Dumper.Dump(OutputFilePath,
								(s, url) => playlistCheckingStarted?.Invoke(this, url),
								(s, chunkCount, newChunkCount, firstChunkId, firstNewChunkId, playlistContent, e, playlistErrorCountInRow) =>
									playlistCheckingFinished?.Invoke(this, chunkCount, newChunkCount, firstChunkId, firstNewChunkId, playlistContent, e, playlistErrorCountInRow),
								(s, count, first, manifestItem) => playlistFirstArrived?.Invoke(this, count, first, manifestItem),
								(s, stream, fn) => outputStreamAssigned?.Invoke(this, stream, fn),
								(s, fn) => outputStreamClosed?.Invoke(this, fn),
								(s, delay, checkingInterval, cycleProcessingTime) =>
									playlistCheckingDelayCalculated?.Invoke(this, delay, checkingInterval, cycleProcessingTime),
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
								(s, fs, e) => dumpingProgress?.Invoke(this, fs, e),
								null, null, null, null, null,
								(s, e, t) =>
								{
									dumpingFinished?.Invoke(this, e, t);
									Dumper = null;
								},
								playlistCheckingIntervalMilliseconds,
								maxPlaylistErrorCountInRow, maxOtherErrorsInRow,
								saveChunksInfo, storeChunkFileName, storeChunkUrl, useGmtTime));
						}
					}

					checkingFinished?.Invoke(this, errorCode);
				});

				IsChecking = false;
			}
		}
	}
}
