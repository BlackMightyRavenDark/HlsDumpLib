using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace HlsDumpLib.GuiTestWPF
{
	public class ViewModelMainWindow : Notifier
	{
		public ObservableCollection<ModelStreamItem> StreamItems { get; }

		public string DownloadingDir { get => _downloadingDir; set => SetProperty(ref _downloadingDir, value); }
		public string StreamTitle { get => _streamTitle; set => SetProperty(ref _streamTitle, value); }
		public string StreamPlaylistUrl { get => _streamPlaylistUrl; set => SetProperty(ref _streamPlaylistUrl, value); }
		public int PlaylistCheckingIntervalMilliseconds
		{
			get => _playlistCheckingIntervalMilliseconds;
			set => SetProperty(ref _playlistCheckingIntervalMilliseconds, value);
		}
		public int PlaylistErrorCountInRowMax { get => _playlistErrorCountInRowMax; set => SetProperty(ref _playlistErrorCountInRowMax, value); }
		public int OtherErrorCountInRowMax { get => _otherErrorCountInRowMax; set => SetProperty(ref _otherErrorCountInRowMax, value); }
		public bool SaveChunksInfo { get => _saveChunksInfo; set => SetProperty(ref _saveChunksInfo, value); }
		public bool SaveChunkFileName { get => _saveChunkFileName; set => SetProperty(ref _saveChunkFileName, value); }
		public bool SaveChunkFileUrl { get => _saveChunkFileUrl; set => SetProperty(ref _saveChunkFileUrl, value); }
		public bool UseGmtTime { get => _useGmtTime; set => SetProperty(ref _useGmtTime, value); }
		public bool StartDumpingImmediately { get => _startDumpingImmediately; set => SetProperty(ref _startDumpingImmediately, value); }

		public ModelStreamItem SelectedItem { get => _selectedItem; set => SetProperty(ref _selectedItem, value); }

		private string _downloadingDir;
		private string _streamTitle;
		private string _streamPlaylistUrl;
		private int _playlistCheckingIntervalMilliseconds = 2000;
		private int _playlistErrorCountInRowMax = 5;
		private int _otherErrorCountInRowMax = 10;
		private bool _saveChunksInfo = true;
		private bool _saveChunkFileName = true;
		private bool _saveChunkFileUrl = true;
		private bool _useGmtTime = true;
		private bool _startDumpingImmediately = true;
		private ModelStreamItem _selectedItem;

		private bool _isClosing = false;

		public ICommand BtnAddStreamCommand { get; }
		public ICommand BtnSelectDownloadingDirCommand { get; }
		public ICommand StartDumpingCommand { get; }
		public ICommand StopDumpingCommand { get; }

		private ListView _listViewStreams;

		public ViewModelMainWindow()
		{
			StreamItems = new ObservableCollection<ModelStreamItem>();

			BtnAddStreamCommand = new LambdaCommand(btnAddStream_Handler);
			BtnSelectDownloadingDirCommand = new LambdaCommand(btnSelectDownloadingDir_Handler);
			StartDumpingCommand = new LambdaCommand(CheckStream, obj => SelectedItem != null && !SelectedItem.IsDumping);
			StopDumpingCommand = new LambdaCommand(
				obj => SelectedItem.Stop(),
				obj => SelectedItem != null && SelectedItem.IsDumping && !SelectedItem.WantsStop);
		}

		public void Initialize(object obj)
		{
			if (obj is Window)
			{
				(obj as Window).Closing += OnWindowClosing;
			}
			if (obj is ListView)
			{
				_listViewStreams = obj as ListView;
			}
		}

		private async void OnWindowClosing(object sender, CancelEventArgs e)
		{
			if (_isClosing)
			{
				e.Cancel = true;
			}
			else if (IsUnfinishedTaskPresent())
			{
				_isClosing = true;
				e.Cancel = true;
				StopAllDumpers();
				await Task.Run(() =>
				{
					bool unfinished = true;
					while (unfinished)
					{
						Thread.Sleep(200);
						Application.Current.Dispatcher.Invoke(() => unfinished = IsUnfinishedTaskPresent());
					}
				});

				_isClosing = false;
				(sender as Window).Close();
			}
		}

		private void btnSelectDownloadingDir_Handler(object obj)
		{
			try
			{
				string dir = obj as string;
				using (System.Windows.Forms.FolderBrowserDialog fbd = new System.Windows.Forms.FolderBrowserDialog())
				{
					fbd.Description = "Выберите папку для скачивания";
					fbd.ShowNewFolderButton = true;
					if (!string.IsNullOrEmpty(dir) && Directory.Exists(dir)) { fbd.SelectedPath = dir; }
					if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
					{
						DownloadingDir = fbd.SelectedPath;
					}
				}
			} catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine(ex.Message);
			}
		}

		private void btnAddStream_Handler(object obj)
		{
			string url = StreamPlaylistUrl?.Trim();
			if (string.IsNullOrEmpty(url))
			{
				MessageBox.Show("Не указана ссылка на плейлист!", "Ошибка!",
					MessageBoxButton.OK, MessageBoxImage.Error);
				return;
			}

			if (!url.StartsWith("http:", StringComparison.OrdinalIgnoreCase) &&
				!url.StartsWith("https:", StringComparison.OrdinalIgnoreCase))
			{
				MessageBox.Show("Указана неправильная ссылка на плейлист!", "Ошибка!",
					MessageBoxButton.OK, MessageBoxImage.Error);
				return;
			}

			string dir = DownloadingDir?.Trim();
			if (string.IsNullOrEmpty(dir))
			{
				MessageBox.Show("Не указана папка для скачивания!", "Ошибка!",
					MessageBoxButton.OK, MessageBoxImage.Error);
				return;
			}

			if (!Directory.Exists(dir))
			{
				MessageBox.Show("Папка для скачивания не найдена!", "Ошибка!",
					MessageBoxButton.OK, MessageBoxImage.Error);
				return;
			}

			string title = GetFixedStreamTitle();
			string outputFileName = FormatOutputFileName(title);

			ModelStreamItem item = new ModelStreamItem()
			{
				Title = title,
				OutputFilePath = Path.Combine(dir, outputFileName),
				PlaylistUrl = url
			};
			StreamItems.Add(item);
			_listViewStreams.ScrollIntoView(item);

			if (StartDumpingImmediately)
			{
				CheckStream(item);
			}
		}

		private void CheckStream(ModelStreamItem streamItem)
		{
			if (streamItem != null)
			{
				if (streamItem.WasStarted)
				{
					const string msg = "Невозможно запустить проверку этого элемента, " +
						"так как ранее он уже был активен. Если проверить его ещё раз, " +
						"то скачанное им видео будет повреждено и потеряет смысл существования!";
					MessageBox.Show(msg, "GUI test (WPF)",
						MessageBoxButton.OK, MessageBoxImage.Exclamation);
					return;
				}

				streamItem.Check(
					PlaylistCheckingIntervalMilliseconds,
					PlaylistErrorCountInRowMax,
					OtherErrorCountInRowMax,
					SaveChunksInfo, SaveChunkFileName, SaveChunkFileUrl,
					UseGmtTime);
			}
		}

		private void CheckStream(object obj)
		{
			CheckStream(SelectedItem);
		}

		private string GetFixedStreamTitle()
		{
			string title = StreamTitle?.Trim();
			return string.IsNullOrEmpty(title) ? "untitled" : Utils.FixFileName(title);
		}

		private string FormatOutputFileName(string title)
		{
			DateTime dateTime = UseGmtTime ? DateTime.UtcNow : DateTime.Now;
			string t = $"{title}_{dateTime:yyyy-MM-dd HH-mm-ss}";
			return UseGmtTime ? $"{t} GMT" : t;
		}

		private void StopAllDumpers()
		{
			foreach (ModelStreamItem item in StreamItems)
			{
				item.Stop();
			}
		}

		private bool IsUnfinishedTaskPresent()
		{
			return StreamItems.Count > 0 &&
				StreamItems.FirstOrDefault(element => element.IsDumping || element.IsChecking) != null;
		}
	}
}
