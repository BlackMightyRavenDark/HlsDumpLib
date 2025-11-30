using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
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
		public M3UManifest Manifest { get; private set; }

		public bool HasHeaderSegment => !string.IsNullOrEmpty(StreamHeaderSegmentUrl) && !string.IsNullOrWhiteSpace(StreamHeaderSegmentUrl);
		public bool HasSegments => Segments != null && Segments.Count > 0;
		public bool IsManifest => Manifest != null;

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
						if (splitted[0] == "#EXT-SERVER")
						{
							PlaylistDate = ExtractDateFromExtServerString(splitted[1]);
						}
						else if (splitted[0] == "#EXT-X-MEDIA-SEQUENCE")
						{
							MediaSequence = int.TryParse(splitted[1], out int id) ? id : -1;
						}
						else if (splitted[0] == "#EXT-X-MAP")
						{
							StreamHeaderSegmentUrl = ExtractUrlFromXMapString(splitted[1]);
						}
						else if (splitted[0] == "#EXT-X-STREAM-INF")
						{
							Manifest = M3UManifest.Parse(PlaylistContent, PlaylistUrl);
							return;
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
			bool playlistDateFound = PlaylistDate != DateTime.MinValue;

			DateTime segmentDate = PlaylistDate;
			int segmentId = MediaSequence < 0 ? 0 : MediaSequence;
			int noDateSegmentCount = 0;

			int stringCount = playlistStrings.Length;
			for (int i = startStringId; i < stringCount; ++i)
			{
				string[] splitted = playlistStrings[i].Split(new char[] { ':' }, 2, StringSplitOptions.None);
				if (splitted[0] == "#EXTINF")
				{
					double segmentLength = 0.0;
					string segmentFileName = null;
					string segmentUrl = null;
					bool segmentDateFound = false;

					if (splitted.Length == 2)
					{
						string[] lengthSplitted = splitted[1].Split(',');
						NumberFormatInfo numberFormatInfo = new NumberFormatInfo() { NumberDecimalSeparator = "." };
						segmentLength = double.TryParse(lengthSplitted[0], NumberStyles.Any,
							numberFormatInfo, out double d) ? d : 0.0;
					}

					if (i > 0)
					{
						string[] s = playlistStrings[i - 1].Split(new char[] { ':' }, 2, StringSplitOptions.None);
						if (s[0] == "#EXT-X-PROGRAM-DATE-TIME")
						{
							if (s.Length == 2 && ExtractDateFromExtProgramDateTime(s[1], out DateTime tmpSegmentDate))
							{
								segmentDate = tmpSegmentDate;
								segmentDateFound = true;

								//Для плейлистов, в которых дата указана, начиная не с первого сегмента.
								if (noDateSegmentCount > 0 && Segments.Count > 0)
								{
									double summaryLength = 0.0;
									for (; noDateSegmentCount > 0; noDateSegmentCount--)
									{
										summaryLength += Segments[noDateSegmentCount - 1].LengthSeconds;
										DateTime previousSegmentDate = segmentDate - TimeSpan.FromSeconds(summaryLength);
										Segments[noDateSegmentCount - 1].SetCreationDate(previousSegmentDate);

										if (!playlistDateFound && noDateSegmentCount == 1)
										{
											PlaylistDate = previousSegmentDate;
											playlistDateFound = true;
										}
									}
								}

								if (!playlistDateFound)
								{
									PlaylistDate = segmentDate;
									playlistDateFound = true;
								}
							}
						}
						else if (!playlistDateFound)
						{
							noDateSegmentCount++;
						}

						if (!segmentDateFound && !firstSegment)
						{
							double lastSegmentLength = Segments.Count > 0 ?
								Segments[Segments.Count - 1].LengthSeconds :
								//Безвыходное положение.
								//Негде взять продолжительность предыдущего сегмента.
								//По-этому, делаем её равной одной десятой секунды.
								0.1;
							segmentDate += TimeSpan.FromSeconds(lastSegmentLength);
						}
					}
					else if (!playlistDateFound)
					{
						noDateSegmentCount++;
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
						bool dateFound = playlistDateFound || segmentDateFound;
						StreamSegment segment = new StreamSegment(segmentDate, segmentLength,
							segmentId, segmentFileName, segmentUrl, !dateFound);
						Segments.Add(segment);
					}

					segmentId++;
					firstSegment = false;
				}
			}
		}

		public IEnumerable<StreamSegment> Filter(IEnumerable<StreamSegment> filter)
		{
			return Segments?.Where(s => !filter.Any(a => a.Url == s.Url));
		}

		/// <summary>
		/// Warning! Playlist must be parsed before calling this method!
		/// </summary>
		/// <returns>Extension for the output file name</returns>
		public string GetOutputFileExtension()
		{
			const string defaultExtension = ".ts";

			if (!HasSegments) { return defaultExtension; }

			if (!string.IsNullOrEmpty(Segments[0].Url) && !string.IsNullOrWhiteSpace(Segments[0].Url))
			{
				string ext = Path.GetExtension(Segments[0].Url);
				if (string.IsNullOrEmpty(ext) || string.IsNullOrWhiteSpace(ext)) { return defaultExtension; }

				return ext.Equals(".pts", StringComparison.OrdinalIgnoreCase) ? defaultExtension : ext;
			}

			return defaultExtension;
		}
	}
}
