using System;
using System.Collections.Generic;
using System.Globalization;

namespace HlsDumpLib
{
	public static class Utils
	{
		public static DateTime EpochToDate(long epoch)
		{
			TimeSpan timeSpan = TimeSpan.FromMilliseconds(epoch);
			DateTime minEpoch = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
			return minEpoch.AddTicks(timeSpan.Ticks);
		}

		public static string ExtractUrlFileName(string fileUrl)
		{
			int n = fileUrl.LastIndexOf('/');
			return n >= 0 ? fileUrl.Substring(n + 1) : null;
		}

		public static string ExtractUrlFilePath(string fileUrl)
		{
			int n = fileUrl.LastIndexOf('/');
			return n > 0 ? fileUrl.Substring(0, n) : null;
		}

		public static Dictionary<string, string> SplitStringToKeyValues(
			string inputString, char keySeparator, char valueSeparator)
		{
			if (string.IsNullOrEmpty(inputString) || string.IsNullOrWhiteSpace(inputString))
			{
				return null;
			}

			string[] keyValues = inputString.Split(keySeparator);
			Dictionary<string, string> dict = new Dictionary<string, string>();
			for (int i = 0; i < keyValues.Length; ++i)
			{
				string[] t = keyValues[i].Split(new char[] { valueSeparator }, 2);
				string value = t.Length > 1 ? t[1] : string.Empty;
				dict.Add(t[0], value);
			}

			return dict;
		}

		internal static string ExtractUrlFromXMapString(string xMapValue)
		{
			string[] splitted = xMapValue?.Split('=');
			return splitted != null && splitted.Length > 1 && !string.IsNullOrEmpty(splitted[1]) ?
				splitted[1].Substring(1, splitted[1].Length - 2) : null;
		}

		internal static DateTime ExtractDateFromExtServerString(string extServerValue)
		{
			try
			{
				Dictionary<string, string> dictionary = SplitStringToKeyValues(extServerValue, ',', '=');
				if (dictionary != null && dictionary.TryGetValue("TIME", out string timeValue))
				{
					if (long.TryParse(timeValue, out long seconds))
					{
						return EpochToDate(seconds);
					}
				}
			}
#if DEBUG
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine(ex.Message);
			}
#else
			catch { }
#endif
			return DateTime.MinValue;
		}

		internal static DateTime ExtractDateFromExtProgramDateTime(string extProgramDateTime)
		{
			if (DateTime.TryParseExact(extProgramDateTime, "yyyy-MM-ddTHH:mm:ss.fffZ",
				null, DateTimeStyles.AssumeLocal, out DateTime dateTime))
			{
				return dateTime.ToUniversalTime();
			}
			if (DateTime.TryParse(extProgramDateTime, null,
				DateTimeStyles.AssumeLocal, out dateTime))
			{
				return dateTime.ToUniversalTime();
			}
			return DateTime.MinValue;
		}

		public static bool IsGmt(this DateTime dateTime)
		{
			return dateTime.Kind == DateTimeKind.Utc;
		}

		public static DateTime ToLocal(this DateTime dateTime)
		{
			return dateTime.IsGmt() ? dateTime.ToLocalTime() : dateTime;
		}
	}
}
