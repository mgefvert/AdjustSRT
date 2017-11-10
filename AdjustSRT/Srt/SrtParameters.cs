using System;
using System.IO;

namespace AdjustSRT.Srt
{
    public class SrtParameters
    {
        public TextReader InFile { get; set; }
        public TextWriter OutFile { get; set; }
        public double Offset { get; set; }
        public double Scale { get; set; }
    }
}
