using System;

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
    }
}
