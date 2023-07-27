using System;
using System.Collections.Generic;
using System.Linq;

namespace HlsDumpLib
{
    public class M3UPlaylist
    {
        public string PlaylistContent { get; }
        public string PlaylistUrl { get; }
        private string _playlistPath;

        public int MediaSequence { get; private set; } = -1;
        public string StreamHeaderSegmentUrl { get; private set; }
        public List<string> Segments { get; private set; }
        public List<string> SubPlaylistUrls { get; private set; }

        public M3UPlaylist(string playlistContent, string playlistUrl)
        {
            PlaylistContent = playlistContent;
            PlaylistUrl = playlistUrl;
            _playlistPath = ExtractUrlFilePath(playlistUrl);
        }

        public void Parse()
        {
            string[] strings = PlaylistContent.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);
            int stringCount = strings.Length;
            for (int i = 0; i < stringCount; ++i)
            {
                if (!string.IsNullOrEmpty(strings[i]) && !string.IsNullOrWhiteSpace(strings[i]))
                {
                    string[] splitted = strings[i].Split(new char[] { ':' }, 2);
                    if (splitted != null && splitted.Length == 2)
                    {
                        if (splitted[0] == "#EXT-X-STREAM-INF")
                        {
                            ParseManifest(strings, i);
                            break;
                        }
                        else if (splitted[0] == "#EXT-X-MEDIA-SEQUENCE")
                        {
                            MediaSequence = int.TryParse(splitted[1], out int id) ? id : -1;
                        }
                        else if (splitted[0] == "#EXT-X-MAP")
                        {
                            StreamHeaderSegmentUrl = ExtractUrlFromXMap(splitted[1]);
                        }
                        else if (splitted[0] == "#EXT-X-PROGRAM-DATE-TIME" ||
                            splitted[0] == "#EXTINF")
                        {
                            ParseSegments(strings, i);
                            break;
                        }
                    }
                }
            }
        }

        private void ParseSegments(string[] playlistStrings, int startStringId)
        {
            Segments = new List<string>();
            int max = playlistStrings.Length;
            for (int i = startStringId; i < max; ++i)
            {
                if (!string.IsNullOrEmpty(playlistStrings[i]) &&
                    !playlistStrings[i].StartsWith("#"))
                {
                    string url = playlistStrings[i].StartsWith("http") ?
                        playlistStrings[i] : $"{_playlistPath}/{playlistStrings[i]}";
                    string[] splittedUrl = url.Split('?');
                    Segments.Add(splittedUrl[0]);
                }
            }
        }

        private void ParseManifest(string[] manifestStrings, int startStringId)
        {
            SubPlaylistUrls = new List<string>();
            int max = manifestStrings.Length - 2;
            for (int i = startStringId; i <= max; i += 2)
            {
                string[] splitted = manifestStrings[i].Split(new char[] { ':' }, 2);
                if (splitted != null && splitted.Length == 2)
                {
                    if (splitted[0] == "#EXT-X-STREAM-INF")
                    {
                        if (!manifestStrings[i + 1].StartsWith("#"))
                        {
                            if (manifestStrings[i + 1].EndsWith("m3u8", StringComparison.OrdinalIgnoreCase))
                            {
                                string url = manifestStrings[i + 1].StartsWith("http", StringComparison.OrdinalIgnoreCase) ?
                                    manifestStrings[i + 1] : $"{_playlistPath}/{manifestStrings[i + 1]}";
                                SubPlaylistUrls.Add(url);
                            }
                        }
                    }
                }
            }
        }

        public IEnumerable<string> Filter(IEnumerable<string> filter)
        {
            return Segments?.Where((s) => !filter.Contains(s));
        }

        private string ExtractUrlFromXMap(string xMapValue)
        {
            string[] splitted = xMapValue.Split('=');
            return splitted != null && splitted.Length > 1 && !string.IsNullOrEmpty(splitted[1]) ?
                splitted[1].Substring(1, splitted[1].Length - 2) : null;
        }

        private string ExtractUrlFilePath(string fileUrl)
        {
            int n = fileUrl.LastIndexOf('/');
            return n > 0 ? fileUrl.Substring(0, n) : null;
        }
    }
}
