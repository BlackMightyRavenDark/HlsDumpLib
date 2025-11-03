using System;
using MultiThreadedDownloaderLib;
using static HlsDumpLib.GuiTestWPF.Utils;

namespace HlsDumpLib.GuiTestWPF
{
	public class ModelStreamItem : Notifier
	{
		public string Title { get => _title; set => SetProperty(ref _title, value); }
		public string OutputFilePath { get => _outputFilePath; set => SetProperty(ref _outputFilePath, value); }
		public long OutputFileSize
		{
			get => _outputFileSize;
			set
			{
				if (_outputFileSize != value)
				{
					_outputFileSize = value;
					RaisePropertyChanged(nameof(OutputFileSize));
					RaisePropertyChanged(nameof(OutputFileSizeFormatted));
				}
			}
		}
		public int NewChunkCount
		{
			get => _newChunkCount;
			set
			{
				if (_newChunkCount != value)
				{
					_newChunkCount = value;
					RaisePropertyChanged(nameof(NewChunkCount));
					RaisePropertyChanged(nameof(NewChunkCountFormatted));
					RaisePropertyChanged(nameof(ChunkProcessingTimeFormatted));
					RaisePropertyChanged(nameof(ChunkIdFormatted));
					RaisePropertyChanged(nameof(ChunkLengthFormatted));
					RaisePropertyChanged(nameof(ChunkFileSizeFormatted));
					RaisePropertyChanged(nameof(ChunkFileName));
					RaisePropertyChanged(nameof(ChunkUrl));
				}
			}
		}
		public int PlaylistDelay
		{
			get => _playlistDelay;
			set
			{
				if (_playlistDelay != value)
				{
					_playlistDelay = value;
					RaisePropertyChanged(nameof(PlaylistDelay));
					RaisePropertyChanged(nameof(PlaylistDelayFormatted));
				}
			}
		}
		public int ChunkProcessingTime
		{
			get => _chunkProcessingTime;
			set
			{
				if (_chunkProcessingTime != value)
				{
					_chunkProcessingTime = value;
					RaisePropertyChanged(nameof(ChunkProcessingTime));
					RaisePropertyChanged(nameof(ChunkProcessingTimeFormatted));
				}
			}
		}
		public int ChunkId
		{
			get => _chunkId;
			set
			{
				if (_chunkId != value)
				{
					_chunkId = value;
					RaisePropertyChanged(nameof(ChunkId));
					RaisePropertyChanged(nameof(ChunkIdFormatted));
				}
			}
		}
		public double ChunkLength
		{
			get => _chunkLength;
			set
			{
				if (_chunkLength != value)
				{
					_chunkLength = value;
					RaisePropertyChanged(nameof(ChunkLength));
					RaisePropertyChanged(nameof(ChunkLengthFormatted));
				}
			}
		}
		public long ChunkFileSize
		{
			get => _chunkFileSize;
			set
			{
				if (_chunkFileSize != value)
				{
					_chunkFileSize = value;
					RaisePropertyChanged(nameof(ChunkFileSize));
					RaisePropertyChanged(nameof(ChunkFileSizeFormatted));
				}
			}
		}
		public string ChunkFileName { get => _chunkFileName; set => SetProperty(ref _chunkFileName, value); }
		public string ChunkUrl { get => _chunkUrl; set => SetProperty(ref _chunkUrl, value); }
		public int FirstChunkId
		{
			get => _firstChunkId;
			set
			{
				if (_firstChunkId != value)
				{
					_firstChunkId = value;
					RaisePropertyChanged(nameof(FirstChunkId));
					RaisePropertyChanged(nameof(FirstChunkIdFormatted));
				}
			}
		}
		public int ProcessedChunkCount
		{
			get => _processedChunkCount;
			set
			{
				if (_processedChunkCount != value)
				{
					_processedChunkCount = value;
					RaisePropertyChanged(nameof(ProcessedChunkCount));
					RaisePropertyChanged(nameof(ProcessedChunkCountFormatted));
				}
			}
		}
		public int LostChunkCount
		{
			get => _lostChunkCount;
			set
			{
				if (_lostChunkCount != value)
				{
					_lostChunkCount = value;
					RaisePropertyChanged(nameof(LostChunkCount));
					RaisePropertyChanged(nameof(LostChunkCountFormatted));
				}
			}
		}
		public string State { get => _state; set => SetProperty(ref _state, value); }
		public DateTime DumpStarted
		{
			get => _dumpStarted;
			set
			{
				if (_dumpStarted != value)
				{
					_dumpStarted = value;
					RaisePropertyChanged(nameof(DumpStarted));
					RaisePropertyChanged(nameof(DumpStartedFormatted));
				}
			}
		}
		public int PlaylistErrorCountInRow
		{
			get => _playlistErrorCountInRow;
			set
			{
				if (_playlistErrorCountInRow != value)
				{
					_playlistErrorCountInRow = value;
					RaisePropertyChanged(nameof(PlaylistErrorCountInRow));
					RaisePropertyChanged(nameof(PlaylistErrorCountInRowFormatted));
				}
			}
		}
		public int ChunkDownloadErrorCount
		{
			get => _chunkDownloadErrorCount;
			set
			{
				if (_chunkDownloadErrorCount != value)
				{
					_chunkDownloadErrorCount = value;
					RaisePropertyChanged(nameof(ChunkDownloadErrorCount));
					RaisePropertyChanged(nameof(ChunkDownloadErrorCountFormatted));
				}
			}
		}
		public int ChunkAppendErrorCount
		{
			get => _chunkAppendErrorCount;
			set
			{
				if (_chunkAppendErrorCount != value)
				{
					_chunkAppendErrorCount = value;
					RaisePropertyChanged(nameof(ChunkAppendErrorCount));
					RaisePropertyChanged(nameof(ChunkAppendErrorCountFormatted));
				}
			}
		}
		public int OtherErrorCountInRow
		{
			get => _otherErrorCountInRow;
			set
			{
				if (_otherErrorCountInRow != value)
				{
					_otherErrorCountInRow = value;
					RaisePropertyChanged(nameof(OtherErrorCountInRow));
					RaisePropertyChanged(nameof(OtherErrorCountInRowFormatted));
				}
			}
		}
		public string Type { get => _streamType; set => SetProperty(ref _streamType, value); }
		public int ProgramId
		{
			get => _streamProgramId;
			set
			{
				if (_streamProgramId != value)
				{
					_streamProgramId = value;
					RaisePropertyChanged(nameof(ProgramId));
					RaisePropertyChanged(nameof(ProgramIdFormatted));
				}
			}
		}
		public string GroupId { get => _streamGroupId; set => SetProperty(ref _streamGroupId, value); }
		public string FormatName { get => _streamFormatName; set => SetProperty(ref _streamFormatName, value); }
		public string ClosedCaptions { get => _streamClosedCaptions; set => SetProperty(ref _streamClosedCaptions, value); }
		public int Bandwidth
		{
			get => _bandwidth;
			set
			{
				if (_bandwidth != value)
				{
					_bandwidth = value;
					RaisePropertyChanged(nameof(Bandwidth));
					RaisePropertyChanged(nameof(BandwidthFormatted));
				}
			}
		}
		public string VideoResolution { get => _streamVideo; set => SetProperty(ref _streamVideo, value); }
		public int VideoFrameRate
		{
			get => _videoFrameRate;
			set
			{
				if (_videoFrameRate != value)
				{
					_videoFrameRate = value;
					RaisePropertyChanged(nameof(VideoFrameRate));
					RaisePropertyChanged(nameof(VideoFrameRateFormatted));
				}
			}
		}
		public string Codecs { get => _codecs; set => SetProperty(ref _codecs, value); }
		public string Language { get => _language; set => SetProperty(ref _language, value); }
		public string PlaylistUrl { get => _playlistUrl; set => SetProperty(ref _playlistUrl, value); }

		private string _title;
		private string _outputFilePath;
		private long _outputFileSize = -1L;
		private int _newChunkCount = -1;
		private int _playlistDelay = -1;
		private int _chunkProcessingTime = -1;
		private int _chunkId = -1;
		private double _chunkLength = -1.0;
		private long _chunkFileSize = -1L;
		private string _chunkFileName;
		private string _chunkUrl;
		private int _firstChunkId = -1;
		private int _processedChunkCount = -1;
		private int _lostChunkCount = -1;
		private DateTime _dumpStarted = DateTime.MaxValue;
		private string _state = "Остановлен";
		private int _playlistErrorCountInRow = -1;
		private int _chunkDownloadErrorCount = -1;
		private int _chunkAppendErrorCount = -1;
		private int _otherErrorCountInRow = -1;
		private string _streamType;
		private int _streamProgramId;
		private string _streamGroupId;
		private string _streamFormatName;
		private string _streamClosedCaptions;
		private int _bandwidth;
		private string _streamVideo;
		private int _videoFrameRate;
		private string _codecs;
		private string _language;
		private string _playlistUrl;

		private int _maxPlaylistErrorCountInRow;
		private int _maxOtherErrorCountInRow;

		#region Formatter properties
		public string OutputFileSizeFormatted => FormatOutputFileSize();
		public string PlaylistDelayFormatted => FormatPlaylistDelay();
		public string NewChunkCountFormatted => FormatChunkCount();
		public string ChunkProcessingTimeFormatted => FormatChunkProcessingTime();
		public string ChunkLengthFormatted => FormatChunkLength();
		public string ChunkIdFormatted => FormatChunkId();
		public string ChunkFileSizeFormatted => FormatChunkFileSize();
		public string FirstChunkIdFormatted => FormatFirstChunkId();
		public string ProcessedChunkCountFormatted => FormatProcessedChunkCount();
		public string LostChunkCountFormatted => FormatLostChunkCount();
		public string DumpStartedFormatted => FormatDateDumpStarted();
		public string PlaylistErrorCountInRowFormatted => FormatPlaylistErrorCountInRow();
		public string ChunkDownloadErrorCountFormatted => FormatChunkDownloadErrorCount();
		public string ChunkAppendErrorCountFormatted => FormatChunkAppendErrorCount();
		public string OtherErrorCountInRowFormatted => FormatOtherErrorCountInRow();
		public string ProgramIdFormatted => FormatProgramId();
		public string BandwidthFormatted => FormatBandwidth();
		public string VideoFrameRateFormatted => FormatVideoFrameRate();
		#endregion

		public HlsDumper Dumper { get; private set; }

		public bool IsDumping => Dumper != null;
		public bool IsChecking { get; private set; }
		public bool IsRemoving { get; private set; }
		public bool WasStarted { get; private set; }
		public bool WantsStop { get; private set; } = false;

		public void Check(
			int playlistCheckIntervalMilliseconds,
			int maxPlaylistErrorCountInRow,
			int maxOtherErrorCountInRow,
			bool saveChunksInfo,
			bool storeChunkFileName,
			bool storeChunkUrl,
			bool useGmtTime)
		{
			if (!WasStarted && !IsDumping && !IsChecking)
			{
				IsChecking = true;
				_maxPlaylistErrorCountInRow = maxPlaylistErrorCountInRow;
				_maxOtherErrorCountInRow = maxOtherErrorCountInRow;
				State = "Проверка...";

				int errorCode = MultiThreadedDownloaderLib.Utils.GetUrlResponseHeaders(PlaylistUrl, null, out _, out _);
				if (errorCode == 200)
				{
					WantsStop = false;
					WasStarted = true;
					DumpStarted = useGmtTime ? DateTime.UtcNow : DateTime.Now;
					Dumper = new HlsDumper(PlaylistUrl);

					ProcessedChunkCount = 0;
					LostChunkCount = 0;
					PlaylistErrorCountInRow = 0;
					ChunkDownloadErrorCount = 0;
					ChunkAppendErrorCount = 0;
					OtherErrorCountInRow = 0;

					Dumper.Dump(OutputFilePath,
						(s, url) => State = "Проверка плейлиста...",
						(s, chunkCount, newChunkCount, firstChunkId, firstNewChunkId, playlistContent, e, playlistErrorCountInRow) =>
						{
							State = $"Плейлист проверен (код: {e})";
							if (e == 200 && newChunkCount > 0)
							{
								NewChunkCount = newChunkCount;
							}
							else
							{
								NewChunkCount = 0;
								ChunkId = -1;
								ChunkLength = -1.0;
								ChunkFileSize = -1L;
								ChunkProcessingTime = -1;
								ChunkFileName = string.Empty;
								ChunkUrl = string.Empty;
							}
							PlaylistErrorCountInRow = playlistErrorCountInRow;
						},
						(s, count, first, manifestItem) =>
						{
							FirstChunkId = first;
							if (manifestItem != null)
							{
								Type = manifestItem.ItemType;
								ProgramId = manifestItem.ProgramId;
								GroupId = manifestItem.GroupId;
								FormatName = manifestItem.Name;
								ClosedCaptions = manifestItem.ClosedCaptions;
								Bandwidth = manifestItem.Bandwidth;
								VideoResolution = $"{manifestItem.VideoResolutionWidth}x{manifestItem.VideoResolutionHeight}";
								VideoFrameRate = manifestItem.VideoFrameRate;
								Codecs = manifestItem.Codecs;
								Language = manifestItem.Language;
							}
							else
							{
								ProgramId = -1;
								Bandwidth = 0;
								VideoFrameRate = 0;
								Type = GroupId = FormatName = ClosedCaptions = VideoResolution = Codecs = Language = string.Empty;
							}
						},
						(s, stream, fn) => OutputFilePath = fn,
						null,
						(s, delay, checkInterval, cycleProcessingTime) => PlaylistDelay = delay,
						(s, chunk) => State = $"Подключение... {chunk.Url}",
						(s, chunk, chunkSize, code) =>
						{
							if (code == 200 && chunk != null)
							{
								ChunkFileName = chunk.FileName;
								ChunkUrl = chunk.Url;
								ChunkId = chunk.Id;
								ChunkLength = chunk.LengthSeconds;
								ChunkFileSize = chunkSize;
							}
							else
							{
								ChunkFileName = string.Empty;
								ChunkUrl = string.Empty;
								ChunkId = -1;
								ChunkLength = 0.0;
								ChunkFileSize = 0L;
								ChunkProcessingTime = -1;
							}
						},
						(s, chunk, chunkSize, sessionChunkId, chunkProcessingTime) =>
						{
							ChunkFileSize = chunkSize;
							ChunkProcessingTime = chunkProcessingTime;
							ProcessedChunkCount = (s as HlsDumper).ProcessedChunkCountTotal;
							OtherErrorCountInRow = 0;
						},
						(s, playlistErrorCountInRow, playlistErrorCountInRowMax,
							otherErrorCountInRow, otherErrorCountInRowMax,
							chunkDownloadErrorCount, chunkAppendErrorCount, lostChunkCount) =>
						{
							PlaylistErrorCountInRow = playlistErrorCountInRow;
							OtherErrorCountInRow = otherErrorCountInRow;
							ChunkDownloadErrorCount = chunkDownloadErrorCount;
							ChunkAppendErrorCount = chunkAppendErrorCount;
							LostChunkCount = lostChunkCount;
						},
						(s, fs, e) =>
						{
							State = "Дампинг...";
							OutputFileSize = fs;
						},
						null, null, null, null, null,
						(s, e, t) =>
						{
							State = WantsStop ? "Остановлен" : "Завершён";
							Dumper = null;
						},
						playlistCheckIntervalMilliseconds,
						maxPlaylistErrorCountInRow, maxOtherErrorCountInRow,
						saveChunksInfo, storeChunkFileName, storeChunkUrl, useGmtTime);
				}
				else
				{
					State = $"Ошибка! Код {errorCode}";
				}

				IsChecking = false;
				if (WantsStop) { Stop(); }
			}
		}

		public void Stop()
		{
			if (IsDumping)
			{
				WantsStop = true;
				State = "Останавливается...";
				Dumper.StopDumping();
			}
		}

		public void MarkAsRemoving()
		{
			IsRemoving = true;
			State = "Удаляется...";
		}

		#region Formatters
		private string FormatOutputFileSize()
		{
			return WasStarted && OutputFileSize >= 0L ? FormatSize(OutputFileSize) : string.Empty;
		}

		private string FormatChunkCount()
		{
			return IsDumping ? $"{NewChunkCount} / {Dumper.CurrentPlaylistChunkCount}" : string.Empty;
		}

		public string FormatPlaylistDelay()
		{
			return IsDumping ? $"{PlaylistDelay}ms / {Dumper.PlaylistCheckIntervalMilliseconds}ms" : string.Empty;
		}

		private string FormatChunkProcessingTime()
		{
			return WasStarted && ChunkProcessingTime >= 0 ? $"{ChunkProcessingTime}ms" : string.Empty;
		}

		private string FormatChunkId()
		{
			return WasStarted && ChunkId >= 0 ? ChunkId.ToString() : string.Empty;
		}

		private string FormatChunkLength()
		{
			return WasStarted && ChunkLength > 0.0 ? $"{ChunkLength} сек." : string.Empty;
		}

		private string FormatChunkFileSize()
		{
			return WasStarted && ChunkFileSize >= 0L ? FormatSize(ChunkFileSize) : string.Empty;
		}

		private string FormatFirstChunkId()
		{
			return WasStarted && FirstChunkId >= 0 ? FirstChunkId.ToString() : string.Empty;
		}

		private string FormatProcessedChunkCount()
		{
			return WasStarted && ProcessedChunkCount >= 0 ? ProcessedChunkCount.ToString() : string.Empty;
		}

		private string FormatLostChunkCount()
		{
			return WasStarted && LostChunkCount >= 0 ? LostChunkCount.ToString() : string.Empty;
		}

		private string FormatDateDumpStarted()
		{
			if (DumpStarted == DateTime.MaxValue) { return string.Empty; }
			return DumpStarted.IsGmt() ? $"{DumpStarted} GMT" : DumpStarted.ToString("yyyy.MM.dd HH:mm:ss");
		}

		private string FormatPlaylistErrorCountInRow()
		{
			return WasStarted ? $"{PlaylistErrorCountInRow} / {_maxPlaylistErrorCountInRow}" : string.Empty;
		}

		private string FormatChunkDownloadErrorCount()
		{
			return WasStarted ? ChunkDownloadErrorCount.ToString() : string.Empty;
		}

		private string FormatChunkAppendErrorCount()
		{
			return WasStarted ? ChunkAppendErrorCount.ToString() : string.Empty;
		}

		private string FormatOtherErrorCountInRow()
		{
			return WasStarted ? $"{OtherErrorCountInRow} / {_maxOtherErrorCountInRow}" : string.Empty;
		}

		private string FormatProgramId()
		{
			return WasStarted && ProgramId >= 0 ? ProgramId.ToString() : string.Empty;
		}

		private string FormatBandwidth()
		{
			return WasStarted && Bandwidth >= 0 ? Bandwidth.ToString() : string.Empty;
		}

		private string FormatVideoFrameRate()
		{
			return WasStarted && VideoFrameRate >= 0 ? VideoFrameRate.ToString() : string.Empty;
		}
		#endregion
	}
}
