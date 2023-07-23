using System;
using System.Collections.Generic;

namespace HlsDumpLib
{
    public class M3UPlaylist
    {
        public string PlaylistContent { get; }

        public int MediaSequence { get; private set; }
        public List<string> Segments { get; private set; }

        public M3UPlaylist(string playlistContent)
        {
            PlaylistContent = playlistContent;
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
                        if (splitted[0] == "#EXT-X-MEDIA-SEQUENCE")
                        {
                            MediaSequence = int.TryParse(splitted[1], out int id) ? id : -1;
                        }
                        else if (splitted[0] == "#EXT-X-PROGRAM-DATE-TIME")
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
            int max = playlistStrings.Length - 2;
            for (int i = startStringId; i <= max; i += 3)
            {
                string[] splitted = playlistStrings[i].Split(new char[] { ':' }, 2);
                if (splitted != null && splitted.Length == 2)
                {
                    if (splitted[0] == "#EXT-X-PROGRAM-DATE-TIME")
                    {
                        if (!playlistStrings[i + 2].StartsWith("#"))
                        {
                            if (playlistStrings[i + 2].StartsWith("http", StringComparison.OrdinalIgnoreCase))
                            {
                                Segments.Add(playlistStrings[i + 2]);
                            }
                        }
                    }
                }
            }
        }
    }
}
