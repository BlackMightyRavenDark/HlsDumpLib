using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using static HlsDumpLib.Utils;

namespace HlsDumpLib
{
    public class M3UPlaylist
    {
        public string PlaylistContent { get; }
        public string PlaylistUrl { get; }
        public DateTime PlaylistDate { get; private set; }

        private string _playlistPath;

        public int MediaSequence { get; private set; } = -1;
        public string StreamHeaderSegmentUrl { get; private set; }
        public List<StreamSegment> Segments { get; private set; }
        public List<string> SubPlaylistUrls { get; private set; }

        public bool UseGmtTime;

        public M3UPlaylist(string playlistContent, string playlistUrl, bool useGmtTime)
        {
            PlaylistContent = playlistContent;
            PlaylistUrl = playlistUrl;
            UseGmtTime = useGmtTime;
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
                        if (splitted[0] == "#EXT-SERVER")
                        {
                            PlaylistDate = ExtractDateFromExtServerString(splitted[1], UseGmtTime);
                        }
                        else if (splitted[0] == "#EXT-X-STREAM-INF")
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
                            StreamHeaderSegmentUrl = ExtractUrlFromXMapString(splitted[1]);
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
            Segments = new List<StreamSegment>();

            bool firstSegment = true;
            bool dateFound = PlaylistDate != DateTime.MinValue;

            DateTime segmentDate = PlaylistDate;
            int segmentId = MediaSequence < 0 ? 0 : MediaSequence;

            int stringCount = playlistStrings.Length;
            for (int i = startStringId; i < stringCount; ++i)
            {
                double segmentLength = 0.0;
                string segmentFileName = null;
                string segmentUrl = null;

                string[] splitted = playlistStrings[i].Split(new char[] { ':' }, 2, StringSplitOptions.None);
                if (splitted[0] == "#EXTINF")
                {
                    if (splitted.Length == 2)
                    {
                        string[] lengthSplitted = splitted[1].Split(',');
                        NumberFormatInfo numberFormatInfo = new NumberFormatInfo() { NumberDecimalSeparator = "." };
                        segmentLength = double.TryParse(lengthSplitted[0], NumberStyles.Any,
                            numberFormatInfo, out double d) ? d : 0.0;
                    }

                    if (i > 0)
                    {
                        DateTime tmpSegmentDate = DateTime.MinValue;

                        string[] s = playlistStrings[i - 1].Split(new char[] { ':' }, 2, StringSplitOptions.None);
                        if (s[0] == "#EXT-X-PROGRAM-DATE-TIME")
                        {
                            if (s.Length == 2)
                            {
                                tmpSegmentDate = ExtractDateFromExtProgramDateTime(s[1], UseGmtTime);
                                if (tmpSegmentDate != DateTime.MinValue) { segmentDate = tmpSegmentDate; }
                                if (!dateFound)
                                {
                                    PlaylistDate = segmentDate;
                                    dateFound = tmpSegmentDate != DateTime.MinValue;
                                }
                            }
                        }

                        if (tmpSegmentDate != DateTime.MinValue)
                        {
                            segmentDate = tmpSegmentDate;
                        }
                        else if (!firstSegment)
                        {
                            segmentDate += TimeSpan.FromSeconds(segmentLength);
                        }
                    }

                    if (i < stringCount - 1)
                    {
                        string url = playlistStrings[i + 1];
                        if (!string.IsNullOrEmpty(url) && !url.StartsWith("#"))
                        {
                            url = url.Split('?')[0];
                            if (url.StartsWith("http"))
                            {
                                int n = url.LastIndexOf('/');
                                segmentFileName = n >= 0 ? url.Substring(n + 1) : null;
                            }
                            else
                            {
                                segmentFileName = url;
                                url = $"{_playlistPath}/{segmentFileName}";
                            }

                            segmentUrl = url;

                            i++;
                        }
                    }

                    if (!string.IsNullOrEmpty(segmentUrl) && !string.IsNullOrWhiteSpace(segmentUrl))
                    {
                        StreamSegment segment = new StreamSegment(segmentDate, segmentLength,
                            segmentId, segmentFileName, segmentUrl);
                        Segments.Add(segment);
                    }

                    segmentId++;
                    firstSegment = false;
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

        public IEnumerable<StreamSegment> Filter(IEnumerable<StreamSegment> filter)
        {
            return Segments?.Where(s => !filter.Any(a => a.Url == s.Url));
        }
    }
}
