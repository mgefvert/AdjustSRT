using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace AdjustSRT
{
    class Program
    {
        private readonly static Regex Timecode = new Regex(@"(?<from>\d\d:\d\d:\d\d,\d+)\s*-->\s*(?<to>\d\d:\d\d:\d\d,\d+)");
        private readonly static string TimecodeFormat = @"hh\:mm\:ss\,fff";

        static int Main(string[] args)
        {
            try
            {
                if (args.Length == 0)
                    return DisplayHelp();

                if (args.Length != 2)
                    throw new Exception("Invalid parameters.");

                var srtFile = args[0];
                if (!File.Exists(srtFile))
                    throw new FileNotFoundException("Unable to locate " + srtFile, srtFile);

                var offset = !args[1].Contains(":") 
                    ? TimeSpan.FromSeconds(double.Parse(args[1], CultureInfo.InvariantCulture)) 
                    : TimeSpan.Parse(args[1], CultureInfo.InvariantCulture);

                return ProcessFile(srtFile, offset);
            }
            catch (Exception ex)
            {
                using (new FgColor(ConsoleColor.Red))
                    Console.WriteLine((ex.GetType() != typeof(Exception) ? ex.GetType().Name + ": " : "") + ex.Message);
                return 1;
            }
        }

        private static int ProcessFile(string filename, TimeSpan offset)
        {
            Console.WriteLine($"Adjusting {filename} by {offset.ToString().TrimEnd('0')}");

            var lines = File.ReadAllLines(filename).ToList();
            for (int i = 0; i < lines.Count; i++)
            {
                var match = Timecode.Match(lines[i]);
                if (!match.Success)
                    continue;

                var from = ParseTime(match.Groups["from"].Value, i+1);
                var to = ParseTime(match.Groups["to"].Value, i+1);
                if (from == null || to == null)
                    continue;

                var from2 = from.Value.Add(offset);
                var to2 = to.Value.Add(offset);
                if (from2.Ticks < 0 || to2.Ticks < 0)
                    Console.WriteLine("Warning: Timecode is negative on line " + (i+1));

                lines[i] = 
                    (from2.Ticks < 0 ? "-" : "") + from2.ToString(TimecodeFormat) +
                    " --> " +
                    (to2.Ticks < 0 ? "-" : "") + to2.ToString(TimecodeFormat);
            }

            var offsetText = offset.TotalSeconds.ToString("+0.000;-0.000", CultureInfo.InvariantCulture)
                .TrimEnd('0')
                .TrimEnd('.');

            var newfilename = Path.GetFileNameWithoutExtension(filename) + offsetText + Path.GetExtension(filename);
            File.WriteAllLines(newfilename, lines);
            Console.WriteLine("Saved new SRT file as " + newfilename);
            return 0;
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

        private static int DisplayHelp()
        {
            Console.WriteLine(@"AdjustSRT <filename> <timestamp>

Filename is the SRT file you want to adjust.
Timestamp is either number of seconds, or a valid timestamp in 'hh:mm:ss.fff'.

The new file will be saved as 'filename-timestamp.srt'.

Example:
  AdjustSRT 'Forbidden Planet.srt' -20           # Adjust with -20 secs
  AdjustSRT 'Forbidden Planet.srt' 00:00:05.400  # Adjust with +5.4 secs
".Replace('\'', '\"'));
            return 1;
        }
    }

    public class FgColor : IDisposable
    {
        private readonly ConsoleColor _savedColor;

        public FgColor(ConsoleColor fg)
        {
            _savedColor = Console.ForegroundColor;
            Console.ForegroundColor = fg;
        }

        public void Dispose()
        {
            Console.ForegroundColor = _savedColor;
        }
    }
}
