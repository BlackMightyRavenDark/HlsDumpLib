using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Web;

namespace HlsDumpLib
{
    public class M3UManifestItem
    {
        public string ItemType { get; private set; }
        public int ProgramId { get; private set; }
        public string GroupId { get; private set; }
        public string Name { get; private set; }
        public string ClosedCaptions { get; private set; }
        public int Bandwidth { get; private set; }
        public int VideoResolutionWidth { get; private set; }
        public int VideoResolutionHeight { get; private set; }
        public int VideoFrameRate { get; private set; }
        public string Codecs { get; private set; }
        public string Language { get; private set; }

        public string XMedia { get; }
        public string XStreamInfo { get; }
        public string PlaylistUrl { get; }
        public Dictionary<string, string> MediaDictionary { get; private set; }
        public Dictionary<string, string> StreamInfoDictionary { get; private set; }

        public bool IsVideo => GetIsVideo();

        public M3UManifestItem(string xMedia, string xStreamInfo, string playlistUrl)
        {
            XMedia = xMedia;
            XStreamInfo = xStreamInfo;
            PlaylistUrl = playlistUrl;
            Parse();
        }

        public M3UManifestItem(M3UManifestItem manifestItem)
            : this(manifestItem.XMedia, manifestItem.XStreamInfo, manifestItem.PlaylistUrl) { }

        private void Parse()
        {
            Reset();

            if (!string.IsNullOrEmpty(XStreamInfo) && !string.IsNullOrWhiteSpace(XStreamInfo))
            {
                string fixedStreamInfo = FixString(XStreamInfo).Replace("\"", string.Empty);
                string[] splittedStreamInfo = fixedStreamInfo.Split(new char[] { ':' }, 2, StringSplitOptions.None);
                if (splittedStreamInfo.Length > 1)
                {
                    StreamInfoDictionary = Utils.SplitStringToKeyValues(splittedStreamInfo[1], ',', '=');
                    if (StreamInfoDictionary != null)
                    {
                        if (StreamInfoDictionary.TryGetValue("PROGRAM-ID", out string t))
                        {
                            if (int.TryParse(t, out int id)) { ProgramId = id; }
                        }

                        if (StreamInfoDictionary.TryGetValue("BANDWIDTH", out t))
                        {
                            if (int.TryParse(t, out int bandwidth)) { Bandwidth = bandwidth; }
                        }

                        if (StreamInfoDictionary.TryGetValue("CODECS", out t))
                        {
                            Codecs = HttpUtility.UrlDecode(t);
                        }

                        if (StreamInfoDictionary.TryGetValue("FRAME-RATE", out t))
                        {
                            NumberFormatInfo numberFormatInfo = new NumberFormatInfo() { NumberDecimalSeparator = "." };
                            if (double.TryParse(t, NumberStyles.Any, numberFormatInfo, out double frameRate))
                            {
                                VideoFrameRate = (int)Math.Round(frameRate);
                            }
                        }

                        if (StreamInfoDictionary.TryGetValue("RESOLUTION", out t))
                        {
                            string[] resolutionSplitted = t.Split('x');
                            if (int.TryParse(resolutionSplitted[0], out int w)) { VideoResolutionWidth = w; }

                            if (t.Length > 1)
                            {
                                if (int.TryParse(resolutionSplitted[1], out int h)) { VideoResolutionHeight = h; }
                            }
                        }

                        if (StreamInfoDictionary.TryGetValue("NAME", out t))
                        {
                            Name = t;
                            if (VideoFrameRate <= 0)
                            {
                                string[] splitted = t.Split(':');
                                if (splitted.Length > 1 && splitted[0] == "FPS")
                                {
                                    NumberFormatInfo numberFormatInfo = new NumberFormatInfo() { NumberDecimalSeparator = "." };
                                    if (double.TryParse(splitted[1], NumberStyles.Any, numberFormatInfo, out double frameRate))
                                    {
                                        VideoFrameRate = (int)Math.Round(frameRate);
                                    }
                                }
                            }
                        }

                        if (StreamInfoDictionary.TryGetValue("LANGUAGE", out t))
                        {
                            Language = t;
                        }

                        if (StreamInfoDictionary.TryGetValue("CLOSED-CAPTIONS", out t))
                        {
                            ClosedCaptions = t;
                        }
                    }
                }
            }

            if (!string.IsNullOrEmpty(XMedia) && !string.IsNullOrWhiteSpace(XMedia))
            {
                string fixedMedia = XMedia.Replace("\"", string.Empty);
                string[] splittedMedia = fixedMedia.Split(new char[] { ':' }, 2, StringSplitOptions.None);
                if (splittedMedia.Length > 1)
                {
                    MediaDictionary = Utils.SplitStringToKeyValues(splittedMedia[1], ',', '=');
                    if (MediaDictionary != null)
                    {
                        if (!string.IsNullOrEmpty(Name) && !string.IsNullOrWhiteSpace(Name) &&
                            MediaDictionary.TryGetValue("NAME", out string t))
                        {
                            Name = t;
                        }

                        if (MediaDictionary.TryGetValue("GROUP-ID", out t))
                        {
                            GroupId = t;
                        }

                        if (MediaDictionary.TryGetValue("TYPE", out t))
                        {
                            ItemType = t;
                        }
                    }
                }
            }
        }

        private static string FixString(string s)
        {
            Regex regex = new Regex("CODECS=\"(.*?)\"");
            MatchCollection matches = regex.Matches(s);
            if (matches.Count > 0 && matches[0].Groups.Count > 1)
            {
                string encoded = HttpUtility.UrlEncode(matches[0].Groups[1].Value);
                string t = s.Remove(matches[0].Groups[1].Index, matches[0].Groups[1].Length);
                return t.Insert(matches[0].Groups[1].Index, encoded);
            }

            return s;
        }

        private bool GetIsVideo()
        {
            return ItemType == "VIDEO" || VideoResolutionWidth > 0 || VideoResolutionHeight > 0;
        }
                
        private void Reset()
        {
            MediaDictionary = StreamInfoDictionary = null;
            ItemType = GroupId = Name = Codecs = ClosedCaptions = null;
            ProgramId = Bandwidth = VideoResolutionWidth = VideoResolutionHeight = VideoFrameRate = -1;
        }

        public override string ToString()
        {
            string t = $"Type: {ItemType}{Environment.NewLine}" +
                $"Program ID: {ProgramId}{Environment.NewLine}" +
                $"Group ID: {GroupId}{Environment.NewLine}" +
                $"Name: {Name}{Environment.NewLine}" +
                $"Bandwidth: {Bandwidth}{Environment.NewLine}";
            if (IsVideo)
            {
                t += $"Resolution: {VideoResolutionWidth}x{VideoResolutionHeight}";
                t += VideoFrameRate > 0 ? $", {VideoFrameRate} fps{Environment.NewLine}" : Environment.NewLine;
            }
            else if (VideoFrameRate > 0)
            {
                t += $"Frame rate: {VideoFrameRate}{Environment.NewLine}";
            }
            t += $"Codecs: {Codecs}{Environment.NewLine}";
            if (!string.IsNullOrEmpty(Language))
            {
                t += $"Language: {Language}{Environment.NewLine}";
            }
            if (!string.IsNullOrEmpty(ClosedCaptions))
            {
                t += $"Closed captions: {ClosedCaptions}{Environment.NewLine}";
            }
            t += $"URL: {PlaylistUrl}{Environment.NewLine}";

            return t;
        }
    }
}
