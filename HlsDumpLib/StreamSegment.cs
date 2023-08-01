using System;

namespace HlsDumpLib
{
    public class StreamSegment
    {
        public DateTime CreationDate { get; }
        public double LengthSeconds { get; }
        public string FileName { get; }
        public string Url { get; }

        public StreamSegment(DateTime creationDate, double lengthSeconds,
            string fileName, string url)
        {
            CreationDate = creationDate;
            LengthSeconds = lengthSeconds;
            FileName = fileName;
            Url = url;
        }
    }
}
