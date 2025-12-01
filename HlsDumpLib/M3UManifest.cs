using System;
using System.Collections.Generic;

namespace HlsDumpLib
{
	public class M3UManifest
	{
		public List<M3UManifestItem> Items { get; }
		public string ErrorText { get; }

		public M3UManifest(IEnumerable<M3UManifestItem> manifestItems, string errorText)
		{
			Items = new List<M3UManifestItem>();

			if (manifestItems != null)
			{
				foreach (M3UManifestItem manifestItem in manifestItems)
				{
					Items.Add(new M3UManifestItem(manifestItem));
				}
			}

			ErrorText = errorText;
		}

		public M3UManifest(IEnumerable<M3UManifestItem> manifestItems) : this(manifestItems, null) { }

		public static M3UManifest Parse(string manifestContent, string manifestUrl)
		{
			LinkedList<M3UManifestItem> manifestItems = new LinkedList<M3UManifestItem>();

			string playlistPath = Utils.ExtractUrlFilePath(manifestUrl);
			string errorText = null;

			string[] strings = manifestContent.Split(new string[] { "\n", "\r\n" }, StringSplitOptions.None);
			int stringCount = strings.Length;
			for (int i = 0; i < stringCount; ++i)
			{
				string[] splitted = strings[i].Split(new char[] { ':' }, 2, StringSplitOptions.None);
				if (splitted[0] == "#EXT-X-STREAM-INF")
				{
					string xMedia = null;
					string xStreamInfo = strings[i];
					string playlistUrl = null;

					if (i > 0)
					{
						string[] s = strings[i - 1].Split(new char[] { ':' }, 2, StringSplitOptions.None);
						if (s[0] == "#EXT-X-MEDIA")
						{
							xMedia = strings[i - 1];
						}
					}

					if (i < stringCount - 1)
					{
						string url = strings[i + 1];
						if (!url.StartsWith("#"))
						{
							playlistUrl = url.StartsWith("http") ? url : $"{playlistPath}/{url}";
						}

						i++;
					}

					if (!string.IsNullOrEmpty(playlistUrl) && !string.IsNullOrWhiteSpace(playlistUrl))
					{
						M3UManifestItem manifestItem = new M3UManifestItem(xMedia, xStreamInfo, playlistUrl);
						manifestItems.AddLast(manifestItem);
					}
				}
				else if (splitted[0] == "#EXT-X-ERROR")
				{
					errorText = splitted[1].Trim();
					break;
				}
			}

			return new M3UManifest(manifestItems, errorText);
		}

		public M3UManifestItem FindItemByGroupId(string groupId)
		{
			foreach (M3UManifestItem item in Items)
			{
				if (item.GroupId == groupId) { return item; }
			}

			return null;
		}

		public void SortByBandwidth()
		{
			if (Items.Count > 1)
			{
				Items.Sort((x, y) => x.Bandwidth > y.Bandwidth ? -1 : 1);
			}
		}

		public void SortByVideoHeight()
		{
			if (Items.Count > 1)
			{
				Items.Sort((x, y) => x.VideoResolutionHeight > y.VideoResolutionHeight ? -1 : 1);
			}
		}

		public override string ToString()
		{
			string t = string.Empty;

			if (Items.Count > 0)
			{
				foreach (M3UManifestItem item in Items)
				{
					t += item.ToString() + Environment.NewLine;
				}
			}

			if (!string.IsNullOrEmpty(ErrorText) && !string.IsNullOrWhiteSpace(ErrorText))
			{
				t += ErrorText + Environment.NewLine;
			}

			return t;
		}
	}
}
