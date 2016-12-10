using System;
using System.IO;
using CommonNetTools;

namespace AdjustSRT
{
    class Program
    {
        static int Main()
        {
            try
            {
                CommandLine.DisplayHelpOnEmpty = true;
                var args = CommandLine.Parse<SrtParameters>();

                if (string.IsNullOrEmpty(args.InFile))
                    throw new Exception("Missing input file");

                if (!File.Exists(args.InFile))
                    throw new Exception($"Input file {args.InFile} not found");

                SrtProcessor.Process(args);
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
    }
}
