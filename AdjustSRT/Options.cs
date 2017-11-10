using System;
using DotNetCommons;

namespace AdjustSRT
{
    public class Options
    {
        [CommandLineOption('i', "in", "Input SRT file")]
        public string InFile { get; set; }

        [CommandLineOption('o', "out", "Output SRT file")]
        public string OutFile { get; set; }

        [CommandLineOption("add", "Add seconds or timestamp (hh:mm:ss)")]
        public string Add { get; set; }

        [CommandLineOption("scale", "Scale time by factor relative to the first timecode found")]
        public string Scale { get; set; }

        [CommandLineOption("sub", "Subtract seconds or timestamp (hh:mm:ss)")]
        public string Subtract { get; set; }

        [CommandLineOption("t1", "Map timepoint 1 (hh:mm:ss.fff=hh:mm:ss.fff)")]
        public string Timepoint1 { get; set; }

        [CommandLineOption("t2", "Map timepoint 1 (hh:mm:ss.fff=hh:mm:ss.fff)")]
        public string Timepoint2 { get; set; }

        public bool UsingTimePoints => !string.IsNullOrEmpty(Timepoint1 + Timepoint2);
    }
}
