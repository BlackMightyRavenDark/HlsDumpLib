using System;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HlsDumpLib.GuiTest
{
    public partial class Form1 : Form
    {
        public const int COLUMN_ID_TITLE = 0;
        public const int COLUMN_ID_FILENAME = 1;
        public const int COLUMN_ID_FILESIZE = 2;
        public const int COLUMN_ID_DATEDUMPSTARTED = 3;
        public const int COLUMN_ID_URL = 4;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            MultiThreadedDownloaderLib.MultiThreadedDownloader.SetMaximumConnectionsLimit(100);
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            string title = textBoxTitle.Text.Trim();
            if (string.IsNullOrEmpty(title) || string.IsNullOrWhiteSpace(title))
            {
                MessageBox.Show("Введите название!", "Ошибка!",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string url = textBoxUrl.Text;
            if (string.IsNullOrEmpty(url) || string.IsNullOrWhiteSpace(url))
            {
                MessageBox.Show("Введите ссылку!", "Ошибка!",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string fileName = FixFileName($"{title}_{DateTime.Now:yyyy-MM-dd HH-mm-ss}.ts");
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
                Invoke((MethodInvoker)delegate { OnCheckingStarted(sender); });
            }
            else
            {
                StreamItem streamItem = (sender as StreamChecker).StreamItem;
                int id = FindStreamItemInListView(streamItem, listViewStreams);
                if (id >= 0)
                {
                    listViewStreams.Items[id].SubItems[COLUMN_ID_FILENAME].Text = "Проверяется...";
                }
            }
        }

        private void OnCheckingFinished(object sender, int errorCode)
        {
            if (InvokeRequired)
            {
                Invoke((MethodInvoker)delegate { OnCheckingFinished(sender, errorCode); });
            }
            else
            {
                StreamItem streamItem = (sender as StreamChecker).StreamItem;
                int id = FindStreamItemInListView(streamItem, listViewStreams);
                if (id >= 0)
                {
                    listViewStreams.Items[id].SubItems[COLUMN_ID_FILENAME].Text =
                        errorCode == 200 ? streamItem.FilePath : $"Ошибка {errorCode}";
                }
                streamItem.IsChecking = false;
            }
        }

        private void OnDumpingStarted(object sender)
        {
            if (InvokeRequired)
            {
                Invoke((MethodInvoker)delegate { OnDumpingStarted(sender); });
            }
            else
            {
                StreamItem streamItem = (sender as StreamChecker).StreamItem;
                int id = FindStreamItemInListView(streamItem, listViewStreams);
                if (id >= 0)
                {
                    listViewStreams.Items[id].SubItems[COLUMN_ID_DATEDUMPSTARTED].Text =
                        streamItem.DumpStarted.ToString("yyyy-MM-dd HH-mm-ss");
                }
            }
        }

        private void OnDumpingFinshed(object sender, int errorCode)
        {
            if (InvokeRequired)
            {
                Invoke((MethodInvoker)delegate { OnDumpingFinshed(sender, errorCode); });
            }
            else
            {
                StreamItem streamItem = (sender as StreamChecker).StreamItem;
                int id = FindStreamItemInListView(streamItem, listViewStreams);
                if (id >= 0)
                {
                    listViewStreams.Items[id].SubItems[COLUMN_ID_FILENAME].Text =
                        errorCode == HlsDumper.DUMPING_ERROR_PLAYLIST_GONE ? "Завершено" : "Отменено";
                }
            }
        }

        private void OnDumpingProgress(object sender, long fileSize, int errorCode)
        {
            if (InvokeRequired)
            {
                Invoke((MethodInvoker)delegate { OnDumpingProgress(sender, fileSize, errorCode); });
            }
            else
            {
                StreamItem streamItem = (sender as StreamChecker).StreamItem;
                int id = FindStreamItemInListView(streamItem, listViewStreams);
                if (id >= 0)
                {
                    listViewStreams.Items[id].SubItems[COLUMN_ID_FILESIZE].Text = FormatSize(fileSize);
                }
            }
        }

        private void CheckItem(int itemId)
        {
            StreamItem streamItem = listViewStreams.Items[itemId].Tag as StreamItem;
            if (!streamItem.IsChecking)
            {
                streamItem.IsChecking = true;
                Task.Run(() =>
                {
                    StreamChecker checker = new StreamChecker() { StreamItem = streamItem };
                    checker.Check(streamItem.FilePath, OnCheckingStarted, OnCheckingFinished,
                        OnDumpingStarted, OnDumpingProgress, OnDumpingFinshed);
                });
            }
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
