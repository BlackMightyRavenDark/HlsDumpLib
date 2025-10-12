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
using Newtonsoft.Json.Linq;

namespace HlsDumpLib.GuiTestWPF
{
	public class ViewModelMainWindow : Notifier
	{
		public ObservableCollection<ModelStreamItem> StreamItems { get; }

		public string DownloadDir { get => _downloadDir; set => SetProperty(ref _downloadDir, value); }
		public string StreamTitle { get => _streamTitle; set => SetProperty(ref _streamTitle, value); }
		public string StreamPlaylistUrl { get => _streamPlaylistUrl; set => SetProperty(ref _streamPlaylistUrl, value); }
		public int PlaylistCheckIntervalMilliseconds
		{
			get => _playlistCheckIntervalMilliseconds;
			set => SetProperty(ref _playlistCheckIntervalMilliseconds, value);
		}
		public int PlaylistErrorCountInRowMax { get => _playlistErrorCountInRowMax; set => SetProperty(ref _playlistErrorCountInRowMax, value); }
		public int OtherErrorCountInRowMax { get => _otherErrorCountInRowMax; set => SetProperty(ref _otherErrorCountInRowMax, value); }
		public bool SaveChunkInfos { get => _saveChunkInfos; set => SetProperty(ref _saveChunkInfos, value); }
		public bool SaveChunkFileName { get => _saveChunkFileName; set => SetProperty(ref _saveChunkFileName, value); }
		public bool SaveChunkFileUrl { get => _saveChunkFileUrl; set => SetProperty(ref _saveChunkFileUrl, value); }
		public bool UseGmtTime { get => _useGmtTime; set => SetProperty(ref _useGmtTime, value); }
		public bool StartDumpImmediately { get => _startDumpImmediately; set => SetProperty(ref _startDumpImmediately, value); }

		public ModelStreamItem SelectedItem { get => _selectedItem; set => SetProperty(ref _selectedItem, value); }

		private string _downloadDir;
		private string _streamTitle;
		private string _streamPlaylistUrl;
		private int _playlistCheckIntervalMilliseconds = 2000;
		private int _playlistErrorCountInRowMax = 5;
		private int _otherErrorCountInRowMax = 10;
		private bool _saveChunkInfos = true;
		private bool _saveChunkFileName = true;
		private bool _saveChunkFileUrl = true;
		private bool _useGmtTime = true;
		private bool _startDumpImmediately = true;
		private ModelStreamItem _selectedItem;

		private bool _isClosing = false;
		private readonly string _configurationFilePath;

		public ICommand BtnAddStreamCommand { get; }
		public ICommand BtnSelectDownloadDirCommand { get; }
		public ICommand StartDumpCommand { get; }
		public ICommand StopDumpCommand { get; }

		private ListView _listViewStreams;

		public ViewModelMainWindow()
		{
			StreamItems = new ObservableCollection<ModelStreamItem>();

			BtnAddStreamCommand = new LambdaCommand(btnAddStream_Handler);
			BtnSelectDownloadDirCommand = new LambdaCommand(btnSelectDownloadDir_Handler);
			StartDumpCommand = new LambdaCommand(CheckStream, obj => SelectedItem != null && !SelectedItem.IsDumping);
			StopDumpCommand = new LambdaCommand(
				obj => SelectedItem.Stop(),
				obj => SelectedItem != null && SelectedItem.IsDumping && !SelectedItem.WantsStop);

			try
			{
				_configurationFilePath = Utils.GetConfigurationFilePath();
				if (File.Exists(_configurationFilePath))
				{
					LoadConfig(_configurationFilePath);
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show($"Не удалось загрузить настройки!{Environment.NewLine}{ex.Message}", "Ошибка!",
					MessageBoxButton.OK, MessageBoxImage.Error);
			}
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
				return;
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
				return;
			}

			if (!string.IsNullOrEmpty(_configurationFilePath) && !string.IsNullOrWhiteSpace(_configurationFilePath))
			{
				SaveConfig(_configurationFilePath);
			}
		}

		private void btnSelectDownloadDir_Handler(object obj)
		{
			try
			{
				using (System.Windows.Forms.FolderBrowserDialog fbd = new System.Windows.Forms.FolderBrowserDialog())
				{
					fbd.Description = "Выберите папку для скачивания";
					fbd.ShowNewFolderButton = true;
					if (!string.IsNullOrEmpty(DownloadDir) && Directory.Exists(DownloadDir))
					{
						fbd.SelectedPath = DownloadDir;
					}

					if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
					{
						DownloadDir = fbd.SelectedPath;
					}
				}
			}
#if DEBUG
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine(ex.Message);
#else
			catch
			{
#endif
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

			string dir = DownloadDir?.Trim();
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

			if (StartDumpImmediately)
			{
				CheckStream(item);
			}
		}

		private void SaveConfig(string filePath)
		{
			JObject json = new JObject()
			{
				["downloadDir"] = DownloadDir,
				["maxPlaylistErrorsInRow"] = PlaylistErrorCountInRowMax,
				["maxOtherErrorsInRow"] = OtherErrorCountInRowMax,
				["playlistCheckInterval"] = PlaylistCheckIntervalMilliseconds,
				["saveChunkInfos"] = SaveChunkInfos,
				["storeChunkFileName"] = SaveChunkFileName,
				["storeChunkUrl"] = SaveChunkFileUrl,
				["useGmtTime"] = UseGmtTime,
				["startDumpImmediately"] = StartDumpImmediately
			};

			if (File.Exists(filePath)) { File.Delete(filePath); }
			File.WriteAllText(filePath, json.ToString());
		}

		private void LoadConfig(string filePath)
		{
			JObject json = JObject.Parse(File.ReadAllText(filePath));

			DownloadDir = json.Value<string>("downloadDir");
			{
				JToken jt = json.Value<JToken>("maxPlaylistErrorsInRow");
				int n = jt != null ? jt.Value<int>() : 5;
				PlaylistErrorCountInRowMax = Utils.Clamp(n, 1, 10);
			}
			{
				JToken jt = json.Value<JToken>("maxOtherErrorsInRow");
				int n = jt != null ? jt.Value<int>() : 5;
				OtherErrorCountInRowMax = Utils.Clamp(n, 1, 10);
			}
			{
				JToken jt = json.Value<JToken>("playlistCheckInterval");
				int n = jt != null ? jt.Value<int>() : 2000;
				PlaylistCheckIntervalMilliseconds = Utils.Clamp(n, 500, 5000);
			}
			{
				JToken jt = json.Value<JToken>("saveChunkInfos");
				SaveChunkInfos = jt != null && jt.Value<bool>();
			}
			{
				JToken jt = json.Value<JToken>("storeChunkFileName");
				SaveChunkFileName = jt != null && jt.Value<bool>();
			}
			{
				JToken jt = json.Value<JToken>("storeChunkUrl");
				SaveChunkFileUrl = jt != null && jt.Value<bool>();
			}
			{
				JToken jt = json.Value<JToken>("useGmtTime");
				UseGmtTime = jt != null && jt.Value<bool>();
			}
			{
				JToken jt = json.Value<JToken>("startDumpImmediately");
				StartDumpImmediately = jt != null && jt.Value<bool>();
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
					PlaylistCheckIntervalMilliseconds,
					PlaylistErrorCountInRowMax,
					OtherErrorCountInRowMax,
					SaveChunkInfos, SaveChunkFileName, SaveChunkFileUrl,
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
