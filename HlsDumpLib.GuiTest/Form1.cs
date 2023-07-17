using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HlsDumpLib.GuiTest
{
    public partial class Form1 : Form
    {
        private bool _isClosing = false;

        public const int COLUMN_ID_TITLE = 0;
        public const int COLUMN_ID_FILENAME = 1;
        public const int COLUMN_ID_FILESIZE = 2;
        public const int COLUMN_ID_NEWCHUNKS = 3;
        public const int COLUMN_ID_DELAY = 4;
        public const int COLUMN_ID_CHUNKTIME = 5;
        public const int COLUMN_ID_CHUNKSIZE = 6;
        public const int COLUMN_ID_FIRSTCHUNK = 7;
        public const int COLUMN_ID_PROCESSEDCHUNKS = 8;
        public const int COLUMN_ID_LOSTCHUNKS = 9;
        public const int COLUMN_ID_DATEDUMPSTARTED = 10;
        public const int COLUMN_ID_STATE = 11;
        public const int COLUMN_ID_PLAYLISTERRORS = 12;
        public const int COLUMN_ID_OTHERERRORS = 13;
        public const int COLUMN_ID_PLAYLISTURL = 14;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            MultiThreadedDownloaderLib.MultiThreadedDownloader.SetMaximumConnectionsLimit(100);
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
            }
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            string url = textBoxUrl.Text;
            if (string.IsNullOrEmpty(url) || string.IsNullOrWhiteSpace(url))
            {
                MessageBox.Show("Введите ссылку!", "Ошибка!",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string title = textBoxTitle.Text?.Trim();
            if (string.IsNullOrEmpty(title))
            {
                title = "untitled";
            }

            string fileName = FixFileName($"{title}_{DateTime.Now:yyyy-MM-dd HH-mm-ss-fff}.ts");
            StreamItem item = new StreamItem()
            {
                Title = title,
                PlaylistUrl = url,
                FilePath = fileName
            };
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
            if (listViewStreams.SelectedIndices.Count > 0)
            {
                int id = listViewStreams.SelectedIndices[0];
                StreamItem streamItem = listViewStreams.Items[id].Tag as StreamItem;
                streamItem.Dumper?.StopDumping();
            }
        }

        private void listViewStreams_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right && listViewStreams.SelectedIndices.Count > 0)
            {
                contextMenuStrip1.Show(Cursor.Position);
            }
        }

        private void listView1_MouseDoubleClick(object sender, MouseEventArgs e)
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
                streamItem.FilePath,
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
                streamItem.PlaylistUrl
            };
            item.SubItems.AddRange(subItems);
            item.Tag = streamItem;
            listViewStreams.Items.Add(item);
        }

        private void OnCheckingStarted(object sender)
        {
            if (InvokeRequired)
            {
                Invoke(new MethodInvoker(() => { OnCheckingStarted(sender); }));
            }
            else
            {
                StreamItem streamItem = (sender as StreamChecker).StreamItem;
                int id = FindStreamItemInListView(streamItem, listViewStreams);
                if (id >= 0)
                {
                    listViewStreams.Items[id].SubItems[COLUMN_ID_STATE].Text = "Проверяется...";
                    listViewStreams.Items[id].SubItems[COLUMN_ID_NEWCHUNKS].Text = null;
                }
            }
        }

        private void OnCheckingFinished(object sender, int errorCode)
        {
            if (InvokeRequired)
            {
                Invoke(new MethodInvoker(() => { OnCheckingFinished(sender, errorCode); }));
            }
            else
            {
                StreamItem streamItem = (sender as StreamChecker).StreamItem;
                int id = FindStreamItemInListView(streamItem, listViewStreams);
                if (id >= 0)
                {
                    listViewStreams.Items[id].SubItems[COLUMN_ID_STATE].Text =
                        errorCode == 200 ? streamItem.IsDumping ? "Дампинг..." : null : $"Ошибка {errorCode}";
                }
                streamItem.IsChecking = false;
            }
        }

        private void OnPlaylistCheckingStarted(object sender, string playlistUrl)
        {
            if (InvokeRequired)
            {
                Invoke(new MethodInvoker(() => { OnPlaylistCheckingStarted(sender, playlistUrl); }));
            }
            else
            {
                StreamItem streamItem = (sender as StreamChecker).StreamItem;
                int id = FindStreamItemInListView(streamItem, listViewStreams);
                if (id >= 0)
                {
                    listViewStreams.Items[id].SubItems[COLUMN_ID_STATE].Text = "Проверка плейлиста...";
                }
            }
        }

        private void OnPlaylistCheckingFinished(object sender,
            int chunkCount, int newChunkCount, long firstChunkId, long firstNewChunkId,
            string playlistContent, int errorCode, int playlistErrorCountInRow)
        {
            if (InvokeRequired)
            {
                Invoke(new MethodInvoker(() =>
                {
                    OnPlaylistCheckingFinished(sender,
                        chunkCount, newChunkCount, firstChunkId, firstNewChunkId,
                        playlistContent, errorCode, playlistErrorCountInRow);
                }));
            }
            else
            {
                StreamItem streamItem = (sender as StreamChecker).StreamItem;
                int id = FindStreamItemInListView(streamItem, listViewStreams);
                if (id >= 0)
                {
                    if (streamItem.IsDumping)
                    {
                        listViewStreams.Items[id].SubItems[COLUMN_ID_NEWCHUNKS].Text =
                            $"{streamItem.Dumper.CurrentPlaylistNewChunkCount} / {streamItem.Dumper.CurrentPlaylistChunkCount}";
                        listViewStreams.Items[id].SubItems[COLUMN_ID_DELAY].Text =
                            $"{streamItem.Dumper.LastDelayValueMilliseconds}ms";
                       listViewStreams.Items[id].SubItems[COLUMN_ID_STATE].Text =
                            $"Плейлист проверен (code: {errorCode})";
                        listViewStreams.Items[id].SubItems[COLUMN_ID_PLAYLISTERRORS].Text =
                            $"{playlistErrorCountInRow} / {streamItem.Dumper.PlaylistErrorCountInRowMax}";
                        listViewStreams.Items[id].SubItems[COLUMN_ID_PLAYLISTERRORS].Text =
                            $"{playlistErrorCountInRow} / {streamItem.Dumper.PlaylistErrorCountInRowMax}";
                        listViewStreams.Items[id].SubItems[COLUMN_ID_OTHERERRORS].Text =
                            $"{streamItem.Dumper.OtherErrorCountInRow} / {streamItem.Dumper.OtherErrorCountInRowMax}";
                    }
                    else
                    {
                        listViewStreams.Items[id].SubItems[COLUMN_ID_NEWCHUNKS].Text = null;
                        listViewStreams.Items[id].SubItems[COLUMN_ID_DELAY].Text = null;
                        listViewStreams.Items[id].SubItems[COLUMN_ID_FIRSTCHUNK].Text = null;
                        listViewStreams.Items[id].SubItems[COLUMN_ID_PROCESSEDCHUNKS].Text = null;
                        listViewStreams.Items[id].SubItems[COLUMN_ID_LOSTCHUNKS].Text = null;
                        listViewStreams.Items[id].SubItems[COLUMN_ID_PLAYLISTERRORS].Text = null;
                    }

                    if (newChunkCount <= 0)
                    {
                        listViewStreams.Items[id].SubItems[COLUMN_ID_CHUNKTIME].Text = null;
                    }
                }
            }
        }

        private void OnPlaylistFirstArrived(object sender, int chunkCount, long firstChunkId)
        {
            if (InvokeRequired)
            {
                Invoke(new MethodInvoker(() => { OnPlaylistFirstArrived(sender, chunkCount, firstChunkId); }));
            }
            else
            {
                StreamItem streamItem = (sender as StreamChecker).StreamItem;
                int id = FindStreamItemInListView(streamItem, listViewStreams);
                if (id >= 0)
                {
                    listViewStreams.Items[id].SubItems[COLUMN_ID_FIRSTCHUNK].Text = firstChunkId.ToString();
                }
            }
        }

        private void OnDumpingStarted(object sender)
        {
            if (InvokeRequired)
            {
                Invoke(new MethodInvoker(() => { OnDumpingStarted(sender); }));
            }
            else
            {
                StreamItem streamItem = (sender as StreamChecker).StreamItem;
                int id = FindStreamItemInListView(streamItem, listViewStreams);
                if (id >= 0)
                {
                    listViewStreams.Items[id].SubItems[COLUMN_ID_STATE].Text = "Дампинг...";
                    listViewStreams.Items[id].SubItems[COLUMN_ID_DATEDUMPSTARTED].Text =
                        streamItem.DumpStarted.ToString("yyyy-MM-dd HH-mm-ss");
                    listViewStreams.Items[id].SubItems[COLUMN_ID_PROCESSEDCHUNKS].Text = "0";
                    listViewStreams.Items[id].SubItems[COLUMN_ID_LOSTCHUNKS].Text = "0";
                }
            }
        }

        private void OnDumpingFinished(object sender, int errorCode)
        {
            if (InvokeRequired)
            {
                Invoke(new MethodInvoker(() => { OnDumpingFinished(sender, errorCode); }));
            }
            else
            {
                StreamItem streamItem = (sender as StreamChecker).StreamItem;
                int id = FindStreamItemInListView(streamItem, listViewStreams);
                if (id >= 0)
                {
                    listViewStreams.Items[id].SubItems[COLUMN_ID_NEWCHUNKS].Text = null;
                    listViewStreams.Items[id].SubItems[COLUMN_ID_DELAY].Text = null;
                    listViewStreams.Items[id].SubItems[COLUMN_ID_CHUNKTIME].Text = null;
                    listViewStreams.Items[id].SubItems[COLUMN_ID_CHUNKSIZE].Text = null;
                    listViewStreams.Items[id].SubItems[COLUMN_ID_STATE].Text =
                        errorCode == HlsDumper.DUMPING_ERROR_PLAYLIST_GONE ? "Завершён" : "Отменён";
                    listViewStreams.Items[id].SubItems[COLUMN_ID_PLAYLISTERRORS].Text =
                        $"{streamItem.Dumper.PlaylistErrorCountInRow} / {streamItem.Dumper.PlaylistErrorCountInRowMax}";
                    listViewStreams.Items[id].SubItems[COLUMN_ID_OTHERERRORS].Text =
                        $"{streamItem.Dumper.OtherErrorCountInRow} / {streamItem.Dumper.OtherErrorCountInRowMax}";
                }
            }
        }

        private void OnNextChunkArrived(object sender, long absoluteChunkId, long sessionChunkId,
            long chunkSize, int chunkProcessingTime, string chunkUrl)
        {
            if (InvokeRequired)
            {
                Invoke(new MethodInvoker(() =>
                {
                    OnNextChunkArrived(sender, absoluteChunkId, sessionChunkId,
                        chunkSize, chunkProcessingTime, chunkUrl);
                }));
            }
            else
            {
                StreamItem streamItem = (sender as StreamChecker).StreamItem;
                int id = FindStreamItemInListView(streamItem, listViewStreams);
                if (id >= 0)
                {
                    listViewStreams.Items[id].SubItems[COLUMN_ID_CHUNKTIME].Text = $"{chunkProcessingTime}ms";
                    listViewStreams.Items[id].SubItems[COLUMN_ID_CHUNKSIZE].Text = FormatSize(chunkSize);
                }
            }

        }

        private void OnDumpingProgress(object sender, long fileSize, int errorCode)
        {
            if (InvokeRequired)
            {
                Invoke(new MethodInvoker(() => { OnDumpingProgress(sender, fileSize, errorCode); }));
            }
            else
            {
                StreamItem streamItem = (sender as StreamChecker).StreamItem;
                int id = FindStreamItemInListView(streamItem, listViewStreams);
                if (id >= 0)
                {
                    listViewStreams.Items[id].SubItems[COLUMN_ID_STATE].Text = "Дампинг...";
                    listViewStreams.Items[id].SubItems[COLUMN_ID_FILESIZE].Text = FormatSize(fileSize);
                    listViewStreams.Items[id].SubItems[COLUMN_ID_PROCESSEDCHUNKS].Text =
                        streamItem.Dumper.ProcessedChunkCountTotal.ToString();
                    listViewStreams.Items[id].SubItems[COLUMN_ID_LOSTCHUNKS].Text =
                        streamItem.Dumper.LostChunkCount.ToString();
                }
            }
        }

        private void CheckItem(int itemId)
        {
            if (!_isClosing)
            {
                StreamItem streamItem = listViewStreams.Items[itemId].Tag as StreamItem;
                if (!streamItem.IsChecking)
                {
                    streamItem.IsChecking = true;
                    listViewStreams.Items[itemId].SubItems[COLUMN_ID_STATE].Text = "Запуск проверки...";
                    bool saveChunksInfo = checkBoxSaveChunksInfo.Checked;
                    int maxPlaylistErrorsInRow = (int)numericUpDownPlaylistErrorCountInRow.Value;
                    int maxOtherErrorsInRow = (int)numericUpDownOtherErrorCountInRow.Value;

                    Task.Run(() =>
                    {
                        StreamChecker checker = new StreamChecker() { StreamItem = streamItem };
                        checker.Check(streamItem.FilePath, OnCheckingStarted, OnCheckingFinished,
                            OnPlaylistCheckingStarted, OnPlaylistCheckingFinished, OnPlaylistFirstArrived,
                            OnDumpingStarted, OnNextChunkArrived, OnDumpingProgress, OnDumpingFinished,
                            saveChunksInfo, maxPlaylistErrorsInRow, maxOtherErrorsInRow);
                    });
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
