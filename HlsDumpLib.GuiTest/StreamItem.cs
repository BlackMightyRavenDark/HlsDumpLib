using System;

namespace HlsDumpLib.GuiTest
{
    internal class StreamItem
    {
        public string Title { get; set; }
        public string PlaylistUrl { get; set; }
        public string FilePath { get; set; }
        public DateTime DumpStarted { get; set; } = DateTime.MaxValue;
        public HlsDumper Dumper { get; set; }
        public bool IsChecking { get; set; }
        public bool IsLive => Dumper != null;
    }
}
