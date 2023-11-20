using System;
using Newtonsoft.Json.Linq;

namespace HlsDumpLib
{
    public class StreamSegment
    {
        public DateTime CreationDate { get; }
        public double LengthSeconds { get; }
        public int Id { get; }
        public string FileName { get; }
        public string Url { get; }
        public bool IsRelativeTime { get; }

        public StreamSegment(DateTime creationDate, double lengthSeconds,
            int id, string fileName, string url, bool isRelativeTime)
        {
            CreationDate = creationDate;
            LengthSeconds = lengthSeconds;
            Id = id;
            FileName = fileName;
            Url = url;
            IsRelativeTime = isRelativeTime;
        }

        public JObject ToJson(long position, long size, bool storeFileName, bool storeUrl, bool useGmtTime)
        {
            JObject json = new JObject();
            json["position"] = position;
            json["size"] = size;
            json["id"] = Id;
            json["length"] = LengthSeconds;
            DateTime dateTime = useGmtTime ? CreationDate : !IsRelativeTime ? CreationDate.ToLocal() : CreationDate;
            json["creationDate"] = dateTime;
            if (storeFileName)
            {
                json["fileName"] = FileName;
            }
            if (storeUrl)
            {
                json["url"] = Url;
            }

            return json;
        }
    }
}
