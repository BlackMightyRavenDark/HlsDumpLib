using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MultiThreadedDownloaderLib;

namespace HlsDumpLib
{
    public class HlsDumper
    {
        public string Url { get; }
        private readonly LinkedList<string> _fileList = new LinkedList<string>();

        public HlsDumper(string url)
        {
            Url = url;
        }

        public async void Dump(
            Action<object, string> playlistChecking,
            Action<object, string> nextFile,
            Action<object, string, int> warning,
            Action<object, string, int> error,
            Action<object> finished)
        {
            FileDownloader playlistDownloader = new FileDownloader() { Url = Url };
            await Task.Run(() =>
            {
                int errorCount = 0;
                do
                {
                    playlistChecking?.Invoke(this, Url);
                    int errorCode = playlistDownloader.DownloadString(out string response);
                    if (errorCode == 200)
                    {
                        List<string> list = FilterPlaylist(ExtractUrlsFromPlaylist(response));
                        if (list != null && list.Count > 0)
                        {
                            foreach (string item in list)
                            {
                                _fileList.AddLast(item);
                                if (_fileList.Count > 50)
                                {
                                    _fileList.RemoveFirst();
                                }
                                nextFile?.Invoke(this, item);
                                errorCount = 0;
                            }
                        }
                        else
                        {
                            errorCount++;
                            warning?.Invoke(this, "No new files detected", errorCount);
                        }
                    }
                    else
                    {
                        errorCount++;
                        error?.Invoke(this, "Playlist is not found", errorCount);
                    }

                    Thread.Sleep(2000);
                } while (errorCount < 5);
            });

            finished?.Invoke(this);
        }

        private List<string> ExtractUrlsFromPlaylist(string playlist)
        {
            List<string> result = new List<string>();
            string[] strings = playlist.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);
            result.AddRange(strings.Where(s => s.StartsWith("http")));
            return result;
        }

        private List<string> FilterPlaylist(List<string> playlist)
        {
            return playlist.Where(s => !_fileList.Contains(s))?.ToList();
        }
    }
}
