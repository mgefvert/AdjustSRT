using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace AdjustSRT
{
    public static class SrtProcessor
    {
        private readonly static Regex Timecode = new Regex(@"(?<from>\d\d:\d\d:\d\d,\d+)\s*-->\s*(?<to>\d\d:\d\d:\d\d,\d+)");
        private readonly static string TimecodeFormat = @"hh\:mm\:ss\,fff";

        public static void Process(SrtParameters parameters)
        {
            var scale = parameters.ScaleValue ?? 1;
            var adjust = parameters.AddValue.Subtract(parameters.SubValue);

            Console.Error.WriteLine("Adjusting {0} : scale={1} offset={2}",
                parameters.InFile, scale.ToString(CultureInfo.InvariantCulture), adjust);

            var result = string.IsNullOrEmpty(parameters.OutFile) 
                ? Console.Out 
                : new StreamWriter(parameters.OutFile, false, Encoding.UTF8);
            try
            {
                using (var reader = new StreamReader(parameters.InFile, Encoding.UTF8))
                {
                    string line;
                    int i = 1;
                    TimeSpan? first = null;
                    while ((line = reader.ReadLine()) != null)
                    {
                        var match = Timecode.Match(line);
                        if (!match.Success)
                        {
                            // Regular line
                            result.WriteLine(line);
                            continue;
                        }

                        var from = ParseTime(match.Groups["from"].Value, i);
                        var to = ParseTime(match.Groups["to"].Value, i);
                        if (from == null || to == null)
                        {
                            // Error in processing... weird.
                            result.WriteLine(line);
                            continue;
                        }


                        if (first == null)
                            first = from.Value;

                        var from2 = ProcessTime(from.Value, first.Value, scale, adjust);
                        var to2 = ProcessTime(to.Value, first.Value, scale, adjust);
                        if (from2.Ticks < 0 || to2.Ticks < 0)
                            Console.WriteLine("Warning: Timecode is negative on line " + (i + 1));

                        result.WriteLine((from2.Ticks < 0 ? "-" : "") + from2.ToString(TimecodeFormat) +
                            " --> " +
                            (to2.Ticks < 0 ? "-" : "") + to2.ToString(TimecodeFormat));

                        i++;
                    }
                }
            }
            finally
            {
                if (!string.IsNullOrEmpty(parameters.OutFile))
                    result.Dispose();
            }
        }

        private static TimeSpan ProcessTime(TimeSpan value, TimeSpan first, double scale, TimeSpan adjust)
        {
            return TimeSpan.FromSeconds((value - first).TotalSeconds * scale).Add(first).Add(adjust);
        }

        private static TimeSpan? ParseTime(string text, int line)
        {
            if (string.IsNullOrEmpty(text))
            {
                Console.WriteLine("Missing timecode on line " + line);
                return null;
            }

            TimeSpan ts;
            if (TimeSpan.TryParseExact(text, TimecodeFormat, CultureInfo.InvariantCulture, out ts))
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
