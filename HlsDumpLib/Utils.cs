using System;
using System.Collections.Generic;
using System.Globalization;

namespace HlsDumpLib
{
    internal static class Utils
    {
        public static DateTime EpochToDate(long epoch)
        {
            TimeSpan timeSpan = TimeSpan.FromMilliseconds(epoch);
            return new DateTime(1970, 1, 1).AddTicks(timeSpan.Ticks);
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

            string[] keyValues = inputString.Split(new char[] { keySeparator }, 2);
            Dictionary<string, string> dict = new Dictionary<string, string>();
            for (int i = 0; i < keyValues.Length; ++i)
            {
                string[] t = keyValues[i].Split(new char[] { valueSeparator }, 2);
                string value = t.Length > 1 ? t[1] : string.Empty;
                dict.Add(t[0], value);
            }

            return dict;
        }

        public static string ExtractUrlFromXMapString(string xMapValue)
        {
            string[] splitted = xMapValue?.Split('=');
            return splitted != null && splitted.Length > 1 && !string.IsNullOrEmpty(splitted[1]) ?
                splitted[1].Substring(1, splitted[1].Length - 2) : null;
        }

        public static DateTime ExtractDateFromExtServerString(string extServerValue)
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
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }

            return DateTime.MinValue;
        }

        public static DateTime ExtractDateFromExtProgramDateTime(string extProgramDateTime)
        {
            if (DateTime.TryParseExact(extProgramDateTime, "yyyy-MM-ddTHH:mm:ss.fffZ",
                null, DateTimeStyles.AssumeLocal, out DateTime dateTime))
            {
                return dateTime;
            }
            if (DateTime.TryParse(extProgramDateTime, null,
                DateTimeStyles.AssumeLocal, out dateTime))
            {
                return dateTime;
            }
            return DateTime.MinValue;
        }
    }
}
