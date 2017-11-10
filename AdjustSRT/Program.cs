using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using AdjustSRT.Srt;
using DotNetCommons;

namespace AdjustSRT
{
    internal class Program
    {
        private static int Main()
        {
            try
            {
                CommandLine.DisplayHelpOnEmpty = true;
                var args = CommandLine.Parse<Options>();

                if (string.IsNullOrEmpty(args.InFile))
                    throw new Exception("Missing input file.");

                if (!File.Exists(args.InFile))
                    throw new Exception($"Input file {args.InFile} not found.");

                var parameters = new SrtParameters
                {
                    Scale = ParseDouble(args.Scale, 1),
                    Offset = (ParseTimeCode(args.Add) - ParseTimeCode(args.Subtract)).TotalSeconds
                };

                if (args.UsingTimePoints)
                {
                    if (!string.IsNullOrEmpty(args.Timepoint1) && !string.IsNullOrEmpty(args.Timepoint2))
                        SrtProcessor.MapTimepoint(parameters, ParseTimePoint(args.Timepoint1), ParseTimePoint(args.Timepoint2));
                    else
                        SrtProcessor.MapTimepoint(parameters, ParseTimePoint(args.Timepoint1 + args.Timepoint2));
                }

                parameters.InFile = string.IsNullOrEmpty(args.InFile) ? Console.In : new StreamReader(args.InFile, Encoding.UTF8);
                try
                {
                    parameters.OutFile = string.IsNullOrEmpty(args.OutFile) ? Console.Out : new StreamWriter(args.OutFile, false, Encoding.UTF8);
                    try
                    {
                        SrtProcessor.Process(parameters);
                    }
                    finally
                    {
                        if (parameters.OutFile != Console.Out)
                            parameters.OutFile.Dispose();
                    }
                }
                finally
                {
                    if (parameters.InFile != Console.In)
                        parameters.InFile.Dispose();
                }

                return 0;
            }
            catch (CommandLineDisplayHelpException ex)
            {
                DisplayHelp(ex.Message);
                return 1;
            }
            catch (Exception ex)
            {
                using (new SetConsoleColor(ConsoleColor.Red))
                    Console.Error.WriteLine((ex.GetType() != typeof(Exception) ? ex.GetType().Name + ": " : "") + ex.Message);

                return 1;
            }
        }

        private static void DisplayHelp(string message)
        {
            Console.Error.WriteLine("AdjustSRT <parameters>");
            Console.Error.WriteLine(message);
            Console.Error.WriteLine(@"
Examples:

AdjustSRT -i infile.srt --sub 20
    Adjust SRT file with -20 secs.

AdjustSRT -i infile.srt --add 00:00:05.400
    Adjust SRT file with +5.4 secs.

AdjustSRT -i infile.srt --scale 1.1 -o out.srt
    Scale infile.srt with 10% and write to out.srt.
");
        }

        private static double ParseDouble(string value, double defaultValue)
        {
            return string.IsNullOrEmpty(value) ? defaultValue : double.Parse(value, CultureInfo.InvariantCulture);
        }

        private static TimeSpan ParseTimeCode(string timecode)
        {
            if (string.IsNullOrEmpty(timecode))
                return TimeSpan.Zero;

            return !timecode.Contains(":")
                ? TimeSpan.FromSeconds(double.Parse(timecode, CultureInfo.InvariantCulture))
                : TimeSpan.Parse(timecode, CultureInfo.InvariantCulture);
        }

        private static SrtTimepoint ParseTimePoint(string timepoint)
        {
            if (string.IsNullOrEmpty(timepoint))
                return null;

            var p = timepoint.Split('=').TrimAndFilter().ToList();
            if (p.Count != 2)
                throw new Exception($"Invalid format in parameter '{timepoint}'");

            return new SrtTimepoint
            {
                Origin = ParseTimeCode(p[0]),
                Target = ParseTimeCode(p[1])
            };
        }
    }
}
