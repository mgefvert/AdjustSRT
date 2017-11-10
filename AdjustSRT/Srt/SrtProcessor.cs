using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace AdjustSRT.Srt
{
    public static class SrtProcessor
    {
        private static readonly Regex Timecode = new Regex(@"(?<from>\d\d:\d\d:\d\d,\d+)\s*-->\s*(?<to>\d\d:\d\d:\d\d,\d+)");
        private static readonly string TimecodeFormat = @"hh\:mm\:ss\,fff";

        public static void MapTimepoint(SrtParameters parameters, SrtTimepoint t)
        {
            parameters.Offset = (t.Target - t.Origin).TotalSeconds;
            parameters.Scale = 1;
        }

        public static void MapTimepoint(SrtParameters parameters, SrtTimepoint t1, SrtTimepoint t2)
        {
            // Difference between original two timepoints
            var dist1 = (t2.Origin - t1.Origin).TotalSeconds;

            // Difference between new two timepoints
            var dist2 = (t2.Target - t2.Target).TotalSeconds;

            // Scale needed to adjust
            parameters.Scale = dist2 / dist1;

            // Adjust offset and compensate for scale
            parameters.Offset = (t1.Target - t1.Origin).TotalSeconds / parameters.Scale;
        }

        public static void Process(SrtParameters parameters)
        {
            Console.Error.WriteLine($"Adjusting for scale={parameters.Scale:r} and offset={parameters.Offset:r}s");

            string line;
            var i = 1;

            while ((line = parameters.InFile.ReadLine()) != null)
                parameters.OutFile.WriteLine(ProcessLine(i++, line, parameters.Scale, parameters.Offset));
        }

        private static string ProcessLine(int lineNo, string line, double scale, double offset)
        {
            var match = Timecode.Match(line);
            if (!match.Success)
            {
                // Regular line
                return line;
            }

            var from = ParseTime(match.Groups["from"].Value, lineNo);
            var to = ParseTime(match.Groups["to"].Value, lineNo);
            if (from == null || to == null)
            {
                // Error in processing... weird.
                return line;
            }

            var from2 = ProcessTime(from.Value, scale, offset);
            var to2 = ProcessTime(to.Value, scale, offset);
            if (from2.Ticks < 0 || to2.Ticks < 0)
                Console.WriteLine($"Warning: Timecode is negative on line {lineNo}");

            return 
                (from2.Ticks < 0 ? "-" : "") + from2.ToString(TimecodeFormat) +
                " --> " +
                (to2.Ticks < 0 ? "-" : "") + to2.ToString(TimecodeFormat);
        }

        private static TimeSpan ProcessTime(TimeSpan value, double scale, double offset)
        {
            return TimeSpan.FromSeconds(value.TotalSeconds * scale + offset);
        }

        private static TimeSpan? ParseTime(string text, int line)
        {
            if (string.IsNullOrEmpty(text))
            {
                Console.WriteLine("Missing timecode on line " + line);
                return null;
            }

            if (TimeSpan.TryParseExact(text, TimecodeFormat, CultureInfo.InvariantCulture, out var ts))
                return ts;

            if (TimeSpan.TryParse(text, out ts))
            {
                Console.WriteLine($"Inexact timecode match on line {line}: {text}");
                return ts;
            }

            Console.WriteLine($"Invalid timecode on line {line}: {text}");
            return null;
        }
    }
}
