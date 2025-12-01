using static HlsDumpLib.HlsDumper;

namespace HlsDumpLib
{
	public class HlsDumperParameters
	{
		public string OutputFilePath { get; set; }
		public int PlaylistCheckIntervalMilliseconds { get; set; }
		public int MaxPlaylistErrorsInRow { get; set; }
		public int MaxOtherErrorsInRow { get; set; }
		public int ConnectionTimeoutMilliseconds { get; set; }
		public bool WriteChunkInfo { get; set; }
		public bool StoreChunkFileName { get; set; }
		public bool StoreChunkUrl { get; set; }
		#region Event delegates
		public PlaylistCheckStartedDelegate PlaylistCheckStarted { get; set; }
		public PlaylistCheckFinishedDelegate PlaylistCheckFinished { get; set; }
		public PlaylistFirstArrivedDelegate PlaylistFirstArrived { get; set; }
		public OutputStreamAssignedDelegate OutputStreamAssigned { get; set; }
		public OutputStreamClosedDelegate OutputStreamClosed { get; set; }
		public PlaylistCheckDelayCalculatedDelegate PlaylistCheckDelayCalculated { get; set; }
		public NextChunkConnectingDelegate NextChunkConnecting { get; set; }
		public NextChunkConnectedDelegate NextChunkConnected { get; set; }
		public NextChunkProcessedDelegate NextChunkProcessed { get; set; }
		public ErrorsUpdatedDelegate ErrorsUpdated { get; set; }
		public ChunkDownloadFailedDelegate ChunkDownloadFailed { get; set; }
		public ChunkAppendFailedDelegate ChunkAppendFailed { get; set; }
		public DumpProgressDelegate DumpProgress { get; set; }
		public DumpMessageDelegate DumpMessage { get; set; }
		public DumpWarningDelegate DumpWarning { get; set; }
		public DumpErrorDelegate DumpError { get; set; }
		public DumpFinishedDelegate DumpFinished { get; set; }
		#endregion
	}
}
