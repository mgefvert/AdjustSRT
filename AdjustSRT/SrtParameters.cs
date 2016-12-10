using System;
using System.Globalization;
using CommonNetTools;

namespace AdjustSRT
{
    public class SrtParameters
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

        public TimeSpan AddValue => ParseTimeCode(Add);
        public TimeSpan SubValue => ParseTimeCode(Subtract);
        public double? ScaleValue => string.IsNullOrEmpty(Scale) ? (double?) null : double.Parse(Scale, CultureInfo.InvariantCulture);

        private static TimeSpan ParseTimeCode(string timecode)
        {
            if (string.IsNullOrEmpty(timecode))
                return TimeSpan.Zero;

            return !timecode.Contains(":")
                ? TimeSpan.FromSeconds(double.Parse(timecode, CultureInfo.InvariantCulture))
                : TimeSpan.Parse(timecode, CultureInfo.InvariantCulture);
        }
    }
}
