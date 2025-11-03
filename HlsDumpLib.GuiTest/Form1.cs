using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json.Linq;

namespace HlsDumpLib.GuiTest
{
	public partial class Form1 : Form
	{
		private bool _isClosing = false;
		private string _configFileName = "config.json";

		public const int COLUMN_ID_TITLE = 0;
		public const int COLUMN_ID_FILENAME = 1;
		public const int COLUMN_ID_FILE_SIZE = 2;
		public const int COLUMN_ID_NEW_CHUNKS = 3;
		public const int COLUMN_ID_PLAYLIST_DELAY = 4;
		public const int COLUMN_ID_CHUNK_PROCESSING_TIME = 5;
		public const int COLUMN_ID_CHUNK_ID = 6;
		public const int COLUMN_ID_CHUNK_LENGTH = 7;
		public const int COLUMN_ID_CHUNK_SIZE = 8;
		public const int COLUMN_ID_CHUNK_FILENAME = 9;
		public const int COLUMN_ID_CHUNK_URL = 10;
		public const int COLUMN_ID_FIRST_CHUNK_ID = 11;
		public const int COLUMN_ID_PROCESSED_CHUNKS = 12;
		public const int COLUMN_ID_LOST_CHUNKS = 13;
		public const int COLUMN_ID_DATE_DUMP_STARTED = 14;
		public const int COLUMN_ID_STATE = 15;
		public const int COLUMN_ID_PLAYLIST_ERRORS = 16;
		public const int COLUMN_ID_CHUNK_DOWNLOAD_ERRORS = 17;
		public const int COLUMN_ID_CHUNK_APPEND_ERRORS = 18;
		public const int COLUMN_ID_OTHER_ERRORS = 19;
		public const int COLUMN_ID_TYPE = 20;
		public const int COLUMN_ID_PROGRAM_ID = 21;
		public const int COLUMN_ID_GROUP_ID = 22;
		public const int COLUMN_ID_FORMAT_NAME = 23;
		public const int COLUMN_ID_CLOSED_CAPTIONS = 24;
		public const int COLUMN_ID_BANDWIDTH = 25;
		public const int COLUMN_ID_VIDEO_RESOLUTION = 26;
		public const int COLUMN_ID_VIDEO_FRAME_RATE = 27;
		public const int COLUMN_ID_CODECS = 28;
		public const int COLUMN_ID_LANGUAGE = 29;
		public const int COLUMN_ID_PLAYLIST_URL = 30;

		public Form1()
		{
			InitializeComponent();
		}

		private void Form1_Load(object sender, EventArgs e)
		{
			MultiThreadedDownloaderLib.Utils.ConnectionLimit = 100;

			//fix scrollbar visibility
			columnHeaderPlaylistUrl.Width += 1;

			if (File.Exists(_configFileName)) { LoadConfig(); }
		}

		private void Form1_FormClosing(object sender, FormClosingEventArgs e)
		{
			if (e.CloseReason != CloseReason.ApplicationExitCall && IsActiveTaskPresent())
			{
				e.Cancel = true;
				if (!_isClosing)
				{
					_isClosing = true;
					StopAll();
					Task.Run(() =>
					{
						bool unfinished = true;
						while (unfinished)
						{
							Invoke(new MethodInvoker(() => unfinished = IsActiveTaskPresent()));
							Thread.Sleep(200);
						}
						BeginInvoke(new MethodInvoker(() => { Close(); }));
					});
				}

				return;
			}

			SaveConfig();
		}

		private void SaveConfig()
		{
			JObject json = new JObject()
			{
				["downloadDir"] = textBoxDownloadDir.Text,
				["maxPlaylistErrorsInRow"] = (int)numericUpDownPlaylistErrorCountInRow.Value,
				["maxOtherErrorsInRow"] = (int)numericUpDownOtherErrorCountInRow.Value,
				["playlistCheckInterval"] = (int)numericUpDownPlaylistCheckInterval.Value,
				["saveChunkInfos"] = checkBoxSaveChunkInfos.Checked,
				["storeChunkFileName"] = checkBoxSaveChunkFileName.Checked,
				["storeChunkUrl"] = checkBoxSaveChunkUrl.Checked,
				["useGmtTime"] = checkBoxUseGmtTime.Checked
			};

			JArray jaColumns = new JArray();
			foreach (ColumnHeader columnHeader in listViewStreams.Columns)
			{
				JObject jColumn = new JObject()
				{
					["displayIndex"] = columnHeader.DisplayIndex,
					["width"] = columnHeader.Width
				};
				jaColumns.Add(jColumn);
			}

			json.Add(new JProperty("columns", jaColumns));

			if (File.Exists(_configFileName)) { File.Delete(_configFileName); }
			File.WriteAllText(_configFileName, json.ToString());
		}

		private void LoadConfig()
		{
			JObject json = JObject.Parse(File.ReadAllText(_configFileName));
			{
				string dir = json.Value<string>("downloadDir");
				if (string.IsNullOrEmpty(dir) || string.IsNullOrWhiteSpace(dir))
				{
					dir = Path.GetDirectoryName(Application.ExecutablePath);
				}
				textBoxDownloadDir.Text = dir;
			}
			{
				JToken jt = json.Value<JToken>("maxPlaylistErrorsInRow");
				numericUpDownPlaylistErrorCountInRow.Value = jt == null ? 5 : jt.Value<int>();
			}
			{
				JToken jt = json.Value<JToken>("maxOtherErrorsInRow");
				numericUpDownOtherErrorCountInRow.Value = jt == null ? 5 : jt.Value<int>();
			}
			{
				JToken jt = json.Value<JToken>("playlistCheckInterval");
				if (jt != null)
				{
					int n = jt.Value<int>();
					int min = (int)numericUpDownPlaylistCheckInterval.Minimum;
					numericUpDownPlaylistCheckInterval.Value = n < min ? min : n;
				}
			}
			{
				JToken jt = json.Value<JToken>("saveChunkInfos");
				if (jt != null)
				{
					checkBoxSaveChunkInfos.Checked = jt.Value<bool>();
				}
			}
			{
				JToken jt = json.Value<JToken>("storeChunkFileName");
				if (jt != null)
				{
					checkBoxSaveChunkFileName.Checked = jt.Value<bool>();
				}
			}
			{
				JToken jt = json.Value<JToken>("storeChunkUrl");
				if (jt != null)
				{
					checkBoxSaveChunkUrl.Checked = jt.Value<bool>();
				}
			}
			{
				JToken jt = json.Value<JToken>("useGmtTime");
				if (jt != null)
				{
					checkBoxUseGmtTime.Checked = jt.Value<bool>();
				}
			}

			JArray jaColumns = json.Value<JArray>("columns");
			if (jaColumns != null)
			{
				int columnCount = listViewStreams.Columns.Count;
				for (int i = 0; i < jaColumns.Count && i < columnCount; ++i)
				{
					JObject jColumn = jaColumns[i] as JObject;
					listViewStreams.Columns[i].DisplayIndex = jColumn.Value<int>("displayIndex");
					int width = jColumn.Value<int>("width");
					listViewStreams.Columns[i].Width = width < 60 ? 60 : width;
				}
			}
		}

		private void btnBrowseDownloadDir_Click(object sender, EventArgs e)
		{
			FolderBrowserDialog fbd = new FolderBrowserDialog();
			fbd.Description = "Выберите папку для скачивания";
			fbd.SelectedPath = textBoxDownloadDir.Text;
			if (fbd.ShowDialog() == DialogResult.OK)
			{
				textBoxDownloadDir.Text = fbd.SelectedPath;
			}
			fbd.Dispose();
		}

		private void btnAddStream_Click(object sender, EventArgs e)
		{
			string url = textBoxUrl.Text;
			if (string.IsNullOrEmpty(url) || string.IsNullOrWhiteSpace(url))
			{
				MessageBox.Show("Введите ссылку!", "Ошибка!",
					MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}

			string title = textBoxStreamTitle.Text?.Trim();
			if (string.IsNullOrEmpty(title))
			{
				title = "untitled";
			}

			string downloadDir = textBoxDownloadDir.Text;
			bool dirIsEmpty = string.IsNullOrEmpty(downloadDir) || string.IsNullOrWhiteSpace(downloadDir);
			if (!dirIsEmpty && !Directory.Exists(downloadDir))
			{
				MessageBox.Show("Папка для скачивания не найдена!", "Ошибка!",
					MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}

			string fileName = checkBoxUseGmtTime.Checked ?
				FixFileName($"{title}_{DateTime.UtcNow:yyyy-MM-dd HH-mm-ss-fff} GMT") :
				FixFileName($"{title}_{DateTime.Now:yyyy-MM-dd HH-mm-ss-fff}");
			string filePath = dirIsEmpty ? fileName : Path.Combine(downloadDir, fileName);

			StreamItem item = new StreamItem(title, url, filePath);
			AddItemToListView(item);
		}

		private void miCheckToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (listViewStreams.SelectedIndices.Count > 0)
			{
				int id = listViewStreams.SelectedIndices[0];
				CheckItem(id);
			}
		}

		private void miCancelToolStripMenuItem_Click(object sender, EventArgs e)
		{
			ListViewItem selectedItem = listViewStreams.SelectedItems.Count > 0 ? listViewStreams.SelectedItems[0] : null;
			if (selectedItem != null)
			{
				StreamItem streamItem = selectedItem.Tag as StreamItem;
				if (streamItem.Dumper != null)
				{
					streamItem.Dumper.StopDumping();
					selectedItem.SubItems[COLUMN_ID_STATE].Text = "Останавливается...";
				}
			}
		}

		private async void miRemoveToolStripMenuItem_Click(object sender, EventArgs e)
		{
			ListViewItem selectedItem = listViewStreams.SelectedItems.Count > 0 ? listViewStreams.SelectedItems[0] : null;
			if (selectedItem != null)
			{
				StreamItem streamItem = selectedItem.Tag as StreamItem;
				if (!streamItem.IsRemoving)
				{
					string msg = "Удалить выбранный элемент?";
					string caption;
					MessageBoxIcon icon = MessageBoxIcon.Question;
					if (streamItem.IsDumping)
					{
						msg += $"{Environment.NewLine}Внимание! Дампинг ещё идёт!";
						caption = "Удалятор недосдампленных стримов";
						icon = MessageBoxIcon.Warning;
					}
					else
					{
						caption = "Удалятор сдампленных стримов";
					}

					if (MessageBox.Show(msg, caption, MessageBoxButtons.YesNo, icon) == DialogResult.Yes)
					{
						selectedItem.SubItems[COLUMN_ID_STATE].Text = "Удаляется...";
						streamItem.IsRemoving = true;
						if (streamItem.IsDumping)
						{
							streamItem.Dumper.StopDumping();
							await Task.Run(() =>
							{
								while (streamItem.IsDumping) { Thread.Sleep(200); }
							});
						}

						listViewStreams.Items.Remove(selectedItem);
					}
				}
			}
		}

		private void listViewStreams_MouseUp(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Right && listViewStreams.SelectedIndices.Count > 0)
			{
				contextMenuStreams.Show(Cursor.Position);
			}
		}

		private void listViewStreams_MouseDoubleClick(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left)
			{
				CheckItem(listViewStreams.SelectedIndices[0]);
			}
		}

		private void AddItemToListView(StreamItem streamItem)
		{
			ListViewItem item = new ListViewItem(streamItem.Title);
			string[] subItems = new string[]
			{
				streamItem.OutputFilePath,
				string.Empty,
				string.Empty,
				string.Empty,
				string.Empty,
				string.Empty,
				string.Empty,
				string.Empty,
				string.Empty,
				string.Empty,
				string.Empty,
				string.Empty,
				string.Empty,
				string.Empty,
				"Остановлен",
				string.Empty,
				string.Empty,
				string.Empty,
				string.Empty,
				string.Empty,
				string.Empty,
				string.Empty,
				string.Empty,
				string.Empty,
				string.Empty,
				string.Empty,
				string.Empty,
				string.Empty,
				string.Empty,
				streamItem.PlaylistUrl
			};
			item.SubItems.AddRange(subItems);
			item.Tag = streamItem;
			listViewStreams.Items.Add(item);
		}

		private void OnCheckStarted(object sender)
		{
			Invoke(new MethodInvoker(() =>
			{
				StreamItem streamItem = sender as StreamItem;
				int id = FindStreamItemInListView(streamItem, listViewStreams);
				if (id >= 0)
				{
					listViewStreams.Items[id].SubItems[COLUMN_ID_STATE].Text = "Проверяется...";
					if (!streamItem.IsDumping)
					{
						listViewStreams.Items[id].SubItems[COLUMN_ID_FILE_SIZE].Text =
						listViewStreams.Items[id].SubItems[COLUMN_ID_NEW_CHUNKS].Text =
						listViewStreams.Items[id].SubItems[COLUMN_ID_PLAYLIST_DELAY].Text =
						listViewStreams.Items[id].SubItems[COLUMN_ID_CHUNK_PROCESSING_TIME].Text =
						listViewStreams.Items[id].SubItems[COLUMN_ID_CHUNK_ID].Text =
						listViewStreams.Items[id].SubItems[COLUMN_ID_CHUNK_LENGTH].Text =
						listViewStreams.Items[id].SubItems[COLUMN_ID_CHUNK_SIZE].Text =
						listViewStreams.Items[id].SubItems[COLUMN_ID_CHUNK_FILENAME].Text =
						listViewStreams.Items[id].SubItems[COLUMN_ID_CHUNK_URL].Text =
						listViewStreams.Items[id].SubItems[COLUMN_ID_FIRST_CHUNK_ID].Text =
						listViewStreams.Items[id].SubItems[COLUMN_ID_PROCESSED_CHUNKS].Text =
						listViewStreams.Items[id].SubItems[COLUMN_ID_LOST_CHUNKS].Text =
						listViewStreams.Items[id].SubItems[COLUMN_ID_DATE_DUMP_STARTED].Text =
						listViewStreams.Items[id].SubItems[COLUMN_ID_PLAYLIST_ERRORS].Text =
						listViewStreams.Items[id].SubItems[COLUMN_ID_CHUNK_DOWNLOAD_ERRORS].Text =
						listViewStreams.Items[id].SubItems[COLUMN_ID_CHUNK_APPEND_ERRORS].Text =
						listViewStreams.Items[id].SubItems[COLUMN_ID_OTHER_ERRORS].Text =
						listViewStreams.Items[id].SubItems[COLUMN_ID_TYPE].Text =
						listViewStreams.Items[id].SubItems[COLUMN_ID_PROGRAM_ID].Text =
						listViewStreams.Items[id].SubItems[COLUMN_ID_GROUP_ID].Text =
						listViewStreams.Items[id].SubItems[COLUMN_ID_FORMAT_NAME].Text =
						listViewStreams.Items[id].SubItems[COLUMN_ID_CLOSED_CAPTIONS].Text =
						listViewStreams.Items[id].SubItems[COLUMN_ID_BANDWIDTH].Text =
						listViewStreams.Items[id].SubItems[COLUMN_ID_VIDEO_RESOLUTION].Text =
						listViewStreams.Items[id].SubItems[COLUMN_ID_VIDEO_FRAME_RATE].Text =
						listViewStreams.Items[id].SubItems[COLUMN_ID_CODECS].Text =
						listViewStreams.Items[id].SubItems[COLUMN_ID_LANGUAGE].Text = null;
					}
				}
			}));
		}

		private void OnCheckFinished(object sender, int errorCode)
		{
			Invoke(new MethodInvoker(() =>
			{
				StreamItem streamItem = sender as StreamItem;
				if (!streamItem.IsRemoving)
				{
					int id = FindStreamItemInListView(streamItem, listViewStreams);
					if (id >= 0)
					{
						listViewStreams.Items[id].SubItems[COLUMN_ID_STATE].Text =
							errorCode == 200 ? "Дампинг..." : $"Ошибка {errorCode}";
					}
				}
			}));
		}

		private void OnPlaylistCheckStarted(object sender, string playlistUrl)
		{
			Invoke(new MethodInvoker(() =>
			{
				StreamItem streamItem = sender as StreamItem;
				int id = FindStreamItemInListView(streamItem, listViewStreams);
				if (id >= 0)
				{
					listViewStreams.Items[id].SubItems[COLUMN_ID_STATE].Text = "Проверка плейлиста...";
				}
			}));
		}

		private void OnPlaylistCheckFinished(object sender,
			int chunkCount, int newChunkCount, int firstChunkId, int firstNewChunkId,
			string playlistContent, int errorCode, int playlistErrorCountInRow)
		{
			Invoke(new MethodInvoker(() =>
			{
				StreamItem streamItem = sender as StreamItem;
				int id = FindStreamItemInListView(streamItem, listViewStreams);
				if (id >= 0)
				{
					listViewStreams.Items[id].SubItems[COLUMN_ID_NEW_CHUNKS].Text =
						$"{streamItem.Dumper.CurrentPlaylistNewChunkCount} / {streamItem.Dumper.CurrentPlaylistChunkCount}";
					listViewStreams.Items[id].SubItems[COLUMN_ID_STATE].Text =
						 $"Плейлист проверен (code: {errorCode})";
					listViewStreams.Items[id].SubItems[COLUMN_ID_PLAYLIST_ERRORS].Text =
						$"{playlistErrorCountInRow} / {streamItem.Dumper.PlaylistErrorCountInRowMax}";

					if (newChunkCount <= 0)
					{
						listViewStreams.Items[id].SubItems[COLUMN_ID_CHUNK_PROCESSING_TIME].Text =
						listViewStreams.Items[id].SubItems[COLUMN_ID_CHUNK_ID].Text =
						listViewStreams.Items[id].SubItems[COLUMN_ID_CHUNK_LENGTH].Text =
						listViewStreams.Items[id].SubItems[COLUMN_ID_CHUNK_SIZE].Text =
						listViewStreams.Items[id].SubItems[COLUMN_ID_CHUNK_FILENAME].Text =
						listViewStreams.Items[id].SubItems[COLUMN_ID_CHUNK_URL].Text = null;
					}
				}
			}));
		}

		private void OnPlaylistFirstArrived(object sender, int chunkCount, int firstChunkId, M3UManifestItem manifestItem)
		{
			Invoke(new MethodInvoker(() =>
			{
				StreamItem streamItem = sender as StreamItem;
				int id = FindStreamItemInListView(streamItem, listViewStreams);
				if (id >= 0)
				{
					listViewStreams.Items[id].SubItems[COLUMN_ID_FIRST_CHUNK_ID].Text = firstChunkId.ToString();
					if (manifestItem != null)
					{
						listViewStreams.Items[id].SubItems[COLUMN_ID_TYPE].Text = manifestItem.ItemType;
						if (manifestItem.ProgramId >= 0)
						{
							listViewStreams.Items[id].SubItems[COLUMN_ID_PROGRAM_ID].Text = manifestItem.ProgramId.ToString();
						}
						listViewStreams.Items[id].SubItems[COLUMN_ID_GROUP_ID].Text = manifestItem.GroupId;
						listViewStreams.Items[id].SubItems[COLUMN_ID_FORMAT_NAME].Text = manifestItem.Name;
						listViewStreams.Items[id].SubItems[COLUMN_ID_CLOSED_CAPTIONS].Text = manifestItem.ClosedCaptions;
						if (manifestItem.Bandwidth >= 0)
						{
							listViewStreams.Items[id].SubItems[COLUMN_ID_BANDWIDTH].Text = manifestItem.Bandwidth.ToString();
						}
						listViewStreams.Items[id].SubItems[COLUMN_ID_VIDEO_RESOLUTION].Text =
							$"{manifestItem.VideoResolutionWidth}x{manifestItem.VideoResolutionHeight}";
						if (manifestItem.VideoFrameRate >= 0)
						{
							listViewStreams.Items[id].SubItems[COLUMN_ID_VIDEO_FRAME_RATE].Text = manifestItem.VideoFrameRate.ToString();
						}
						listViewStreams.Items[id].SubItems[COLUMN_ID_CODECS].Text = manifestItem.Codecs;
						listViewStreams.Items[id].SubItems[COLUMN_ID_LANGUAGE].Text = manifestItem.Language;
					}
				}
				else
				{
					listViewStreams.Items[id].SubItems[COLUMN_ID_TYPE].Text =
					listViewStreams.Items[id].SubItems[COLUMN_ID_PROGRAM_ID].Text =
					listViewStreams.Items[id].SubItems[COLUMN_ID_GROUP_ID].Text =
					listViewStreams.Items[id].SubItems[COLUMN_ID_FORMAT_NAME].Text =
					listViewStreams.Items[id].SubItems[COLUMN_ID_CLOSED_CAPTIONS].Text =
					listViewStreams.Items[id].SubItems[COLUMN_ID_BANDWIDTH].Text =
					listViewStreams.Items[id].SubItems[COLUMN_ID_VIDEO_RESOLUTION].Text =
					listViewStreams.Items[id].SubItems[COLUMN_ID_VIDEO_FRAME_RATE].Text =
					listViewStreams.Items[id].SubItems[COLUMN_ID_CODECS].Text =
					listViewStreams.Items[id].SubItems[COLUMN_ID_LANGUAGE].Text = null;
				}
			}));
		}

		public void OnPlaylistCheckDelayCalculated(object sender,
			int delay, int checkInterval, int cycleProcessingTime)
		{
			Invoke(new MethodInvoker(() =>
			{
				StreamItem streamItem = sender as StreamItem;
				int id = FindStreamItemInListView(streamItem, listViewStreams);
				if (id >= 0)
				{
					listViewStreams.Items[id].SubItems[COLUMN_ID_PLAYLIST_DELAY].Text =
						$"{delay}ms / {checkInterval}ms";
				}
			}));
		}

		private void OnDumpStarted(object sender)
		{
			Invoke(new MethodInvoker(() =>
			{
				StreamItem streamItem = sender as StreamItem;
				int id = FindStreamItemInListView(streamItem, listViewStreams);
				if (id >= 0)
				{
					listViewStreams.Items[id].SubItems[COLUMN_ID_STATE].Text = "Дампинг...";
					listViewStreams.Items[id].SubItems[COLUMN_ID_DATE_DUMP_STARTED].Text =
						DateTimeToString(streamItem.DumpStarted);
					listViewStreams.Items[id].SubItems[COLUMN_ID_PROCESSED_CHUNKS].Text =
					listViewStreams.Items[id].SubItems[COLUMN_ID_LOST_CHUNKS].Text =
					listViewStreams.Items[id].SubItems[COLUMN_ID_CHUNK_DOWNLOAD_ERRORS].Text =
					listViewStreams.Items[id].SubItems[COLUMN_ID_CHUNK_APPEND_ERRORS].Text = "0";
					listViewStreams.Items[id].SubItems[COLUMN_ID_OTHER_ERRORS].Text =
						$"0 / {streamItem.Dumper.OtherErrorCountInRowMax}";
				}
			}));
		}

		private void OnDumpFinished(object sender, int errorCode, string errorText)
		{
			Invoke(new MethodInvoker(() =>
			{
				StreamItem streamItem = sender as StreamItem;
				int id = FindStreamItemInListView(streamItem, listViewStreams);
				if (id >= 0)
				{
					listViewStreams.Items[id].SubItems[COLUMN_ID_NEW_CHUNKS].Text =
					listViewStreams.Items[id].SubItems[COLUMN_ID_PLAYLIST_DELAY].Text =
					listViewStreams.Items[id].SubItems[COLUMN_ID_CHUNK_PROCESSING_TIME].Text =
					listViewStreams.Items[id].SubItems[COLUMN_ID_CHUNK_SIZE].Text = null;

					string t;
					switch (errorCode)
					{
						case HlsDumper.DUMP_ERROR_PLAYLIST_GONE:
							t = "Завершён";
							break;

						case HlsDumper.DUMP_ERROR_CANCELED:
							t = "Отменён";
							break;

						case HlsDumper.DUMP_ERROR_NO_FILE_NAME_SPECIFIED:
							t = "Не указано имя файла";
							break;

						case HlsDumper.DUMP_ERROR_MANIFEST_HAS_NO_PLAYLISTS:
							t = "Плейлисты не найдены";
							break;

						default:
							t = null;
							break;
					}

					if (!string.IsNullOrEmpty(errorText))
					{
						if (string.IsNullOrEmpty(t))
						{
							t = errorText;
						}
						else
						{
							t += $" ({errorText})";
						}
					}

					listViewStreams.Items[id].SubItems[COLUMN_ID_STATE].Text = t;
				}
			}));
		}

		private void OnNextChunkConnecting(object sender, StreamSegment chunk)
		{
			Invoke(new MethodInvoker(() =>
			{
				StreamItem streamItem = sender as StreamItem;
				int id = FindStreamItemInListView(streamItem, listViewStreams);
				if (id >= 0)
				{
					listViewStreams.Items[id].SubItems[COLUMN_ID_CHUNK_SIZE].Text = $"Подключение... {chunk.Url}";
				}
			}));
		}

		private void OnNextChunkConnected(object sender, StreamSegment chunk, long chunkFileSize, int errorCode)
		{
			Invoke(new MethodInvoker(() =>
			{
				StreamItem streamItem = sender as StreamItem;
				int id = FindStreamItemInListView(streamItem, listViewStreams);
				if (id >= 0)
				{
					if (errorCode == 200)
					{
						string t = chunkFileSize >= 0L ?
							$"{FormatSize(chunkFileSize)} скачивание... {chunk.Url}" :
							$"Скачивание... {chunk.Url}";
						listViewStreams.Items[id].SubItems[COLUMN_ID_CHUNK_SIZE].Text = t;
					}
					else
					{
						listViewStreams.Items[id].SubItems[COLUMN_ID_FILE_SIZE].Text = $"Ошибка {errorCode}";
						listViewStreams.Items[id].SubItems[COLUMN_ID_CHUNK_LENGTH].Text =
						listViewStreams.Items[id].SubItems[COLUMN_ID_CHUNK_FILENAME].Text =
						listViewStreams.Items[id].SubItems[COLUMN_ID_CHUNK_URL].Text = null;
					}
				}
			}));
		}

		private void OnNextChunkProcessed(object sender, StreamSegment chunk,
			long chunkSize, int sessionChunkId, int chunkProcessingTime)
		{
			Invoke(new MethodInvoker(() =>
			{
				StreamItem streamItem = sender as StreamItem;
				int id = FindStreamItemInListView(streamItem, listViewStreams);
				if (id >= 0)
				{
					listViewStreams.Items[id].SubItems[COLUMN_ID_CHUNK_PROCESSING_TIME].Text = $"{chunkProcessingTime}ms";
					string chunkSizeString = chunkSize >= 0L ? FormatSize(chunkSize) : "<unknown>";
					listViewStreams.Items[id].SubItems[COLUMN_ID_CHUNK_SIZE].Text = chunkSizeString;
					if (chunk != null)
					{
						listViewStreams.Items[id].SubItems[COLUMN_ID_CHUNK_ID].Text = chunk.Id.ToString();
						listViewStreams.Items[id].SubItems[COLUMN_ID_CHUNK_LENGTH].Text = chunk.LengthSeconds.ToString();
						listViewStreams.Items[id].SubItems[COLUMN_ID_CHUNK_FILENAME].Text = chunk.FileName;
						listViewStreams.Items[id].SubItems[COLUMN_ID_CHUNK_URL].Text = chunk.Url;
					}
					else
					{
						listViewStreams.Items[id].SubItems[COLUMN_ID_CHUNK_ID].Text =
						listViewStreams.Items[id].SubItems[COLUMN_ID_CHUNK_LENGTH].Text =
						listViewStreams.Items[id].SubItems[COLUMN_ID_CHUNK_FILENAME].Text =
						listViewStreams.Items[id].SubItems[COLUMN_ID_CHUNK_URL].Text = "null";
					}
				}
			}));
		}

		private void OnDumpProgress(object sender, long fileSize, int errorCode)
		{
			Invoke(new MethodInvoker(() =>
			{
				StreamItem streamItem = sender as StreamItem;
				int id = FindStreamItemInListView(streamItem, listViewStreams);
				if (id >= 0)
				{
					listViewStreams.Items[id].SubItems[COLUMN_ID_STATE].Text = "Дампинг...";
					listViewStreams.Items[id].SubItems[COLUMN_ID_FILE_SIZE].Text = FormatSize(fileSize);
					listViewStreams.Items[id].SubItems[COLUMN_ID_PROCESSED_CHUNKS].Text =
						streamItem.Dumper.ProcessedChunkCountTotal.ToString();
				}
			}));
		}

		private void OnErrorsUpdated(object sender,
			int playlistErrorCountInRow, int playlistErrorCountInRowMax,
			int otherErrorCountInRow, int otherErrorCountInRowMax,
			int chunkDownloadErrorCount, int chunkAppendErrorCount,
			int lostChunkCount)
		{
			Invoke(new MethodInvoker(() =>
			{
				StreamItem streamItem = sender as StreamItem;
				int id = FindStreamItemInListView(streamItem, listViewStreams);
				if (id >= 0)
				{
					listViewStreams.Items[id].SubItems[COLUMN_ID_LOST_CHUNKS].Text = lostChunkCount.ToString();
					listViewStreams.Items[id].SubItems[COLUMN_ID_PLAYLIST_ERRORS].Text =
						$"{playlistErrorCountInRow} / {playlistErrorCountInRowMax}";
					listViewStreams.Items[id].SubItems[COLUMN_ID_CHUNK_DOWNLOAD_ERRORS].Text =
						chunkDownloadErrorCount.ToString();
					listViewStreams.Items[id].SubItems[COLUMN_ID_CHUNK_APPEND_ERRORS].Text =
						chunkAppendErrorCount.ToString();
					listViewStreams.Items[id].SubItems[COLUMN_ID_OTHER_ERRORS].Text =
						$"{otherErrorCountInRow} / {otherErrorCountInRowMax}";
				}
			}));
		}

		private void OnOutputStreamAssigned(object sender, Stream stream, string fileName)
		{
			Invoke(new MethodInvoker(() =>
			{
				StreamItem streamItem = sender as StreamItem;
				int id = FindStreamItemInListView(streamItem, listViewStreams);
				if (id >= 0)
				{
					listViewStreams.Items[id].SubItems[COLUMN_ID_FILENAME].Text = fileName;
				}
			}));
		}

		private void CheckItem(int itemId)
		{
			if (!_isClosing)
			{
				StreamItem streamItem = listViewStreams.Items[itemId].Tag as StreamItem;
				if (!streamItem.IsChecking && !streamItem.IsDumping)
				{
					listViewStreams.Items[itemId].SubItems[COLUMN_ID_STATE].Text = "Запуск проверки...";
					bool saveChunksInfo = checkBoxSaveChunkInfos.Checked;
					bool storeChunkFileName = checkBoxSaveChunkFileName.Checked;
					bool storeChunkUrl = checkBoxSaveChunkUrl.Checked;
					bool useGmtTime = checkBoxUseGmtTime.Checked;
					int maxPlaylistErrorsInRow = (int)numericUpDownPlaylistErrorCountInRow.Value;
					int maxOtherErrorsInRow = (int)numericUpDownOtherErrorCountInRow.Value;
					int playlistCheckIntervalMilliseconds = (int)numericUpDownPlaylistCheckInterval.Value;

					Task.Run(() =>
						streamItem.Check(OnCheckStarted, OnCheckFinished,
							OnPlaylistCheckStarted, OnPlaylistCheckFinished, OnPlaylistFirstArrived,
							OnOutputStreamAssigned, null,
							OnPlaylistCheckDelayCalculated, OnDumpStarted,
							OnNextChunkConnecting, OnNextChunkConnected, OnNextChunkProcessed,
							OnErrorsUpdated, OnDumpProgress, OnDumpFinished,
							playlistCheckIntervalMilliseconds,
							maxPlaylistErrorsInRow, maxOtherErrorsInRow,
							saveChunksInfo, storeChunkFileName, storeChunkUrl, useGmtTime)
					);
				}
			}
		}

		private void StopAll()
		{
			for (int i = 0; i < listViewStreams.Items.Count; ++i)
			{
				StreamItem streamItem = listViewStreams.Items[i].Tag as StreamItem;
				if (streamItem.IsDumping)
				{
					listViewStreams.Items[i].SubItems[COLUMN_ID_STATE].Text = "Останавливается...";
					streamItem.Dumper.StopDumping();
				}
			}
		}

		private bool IsActiveTaskPresent()
		{
			foreach (ListViewItem listViewItem in listViewStreams.Items)
			{
				StreamItem streamItem = listViewItem.Tag as StreamItem;
				if (streamItem.IsChecking || streamItem.IsDumping)
				{
					return true;
				}
			}
			return false;
		}

		private static int FindStreamItemInListView(StreamItem streamItem, ListView listView)
		{
			for (int i = 0; i < listView.Items.Count; ++i)
			{
				if ((listView.Items[i].Tag as StreamItem) == streamItem)
				{
					return i;
				}
			}
			return -1;
		}

		public static string DateTimeToString(DateTime dateTime, string format = "yyyy-MM-dd HH-mm-ss")
		{
			string t = dateTime.ToString(format);
			return dateTime.IsGmt() ? $"{t} GMT" : t;
		}

		public static string FormatSize(long n)
		{
			const int KB = 1000;
			const int MB = 1000000;
			const int GB = 1000000000;
			const long TB = 1000000000000;
			long b = n % KB;
			long kb = (n % MB) / KB;
			long mb = (n % GB) / MB;
			long gb = (n % TB) / GB;

			if (n >= 0 && n < KB)
				return string.Format("{0} B", b);
			if (n >= KB && n < MB)
				return string.Format("{0},{1:D3} KB", kb, b);
			if (n >= MB && n < GB)
				return string.Format("{0},{1:D3} MB", mb, kb);
			if (n >= GB && n < TB)
				return string.Format("{0},{1:D3},{2:D3} GB", gb, mb, kb);

			return string.Format("{0} {1:D3} {2:D3} {3:D3} bytes", gb, mb, kb, b);
		}

		public static string FixFileName(string fn)
		{
			return fn.Replace("\\", "\u29F9").Replace("|", "\u2758").Replace("/", "\u2044")
				.Replace("?", "\u2753").Replace(":", "\uFE55").Replace("<", "\u227A").Replace(">", "\u227B")
				.Replace("\"", "\u201C").Replace("*", "\uFE61").Replace("^", "\u2303").Replace("\n", " ");
		}
	}
}
