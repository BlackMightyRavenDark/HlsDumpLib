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

        public StreamSegment(DateTime creationDate, double lengthSeconds,
            int id, string fileName, string url)
        {
            CreationDate = creationDate;
            LengthSeconds = lengthSeconds;
            Id = id;
            FileName = fileName;
            Url = url;
        }

        public JObject ToJson(long position, long size)
        {
            JObject json = new JObject();
            json["position"] = position;
            json["size"] = size;
            json["id"] = Id;
            json["length"] = LengthSeconds;
            json["creationDate"] = CreationDate;
            json["fileName"] = FileName;
            json["url"] = Url;

            return json;
        }
    }
}
