﻿namespace YTAutoMusic
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine();
            Console.WriteLine(Resources.splash);
            Console.WriteLine();

            (string dlpPath, string ffmpegPath) = FindDependencies();

            if (args.Length != 0)
            {
                TextReader reader = new StreamReader(string.Join("\n", args));
                Console.SetIn(reader);
            }

            ReadArguments(dlpPath, ffmpegPath);
        }

        static void ReadArguments(string dlpPath, string ffmpegPath)
        {
            string response;

            do
            {
                Console.WriteLine(Resources.responses);
                response = Console.ReadLine();

                switch (response)
                {
                    case "q":
                        Environment.Exit(0);
                        break;
                    case "n":
                        PlaylistDownloader.Create(dlpPath, ffmpegPath);
                        break;
                    case "a":
                        PlaylistDownloader.Append(dlpPath, ffmpegPath);
                        break;
                    case "c":
                        PlaylistCopier.Copy();
                        break;
                    case "h":
                        Console.WriteLine(Resources.helpText);
                        break;
                    default:
                        Console.WriteLine("Invalid Response.\n");
                        break;
                }

            } while (true);
        }

        private static (string, string) FindDependencies()
        {
            FileInfo processPath = new(Environment.ProcessPath);
            DirectoryInfo directory = processPath.Directory;

            string dlpPath = Path.Combine(directory.FullName, "Dependencies", "yt-dlp.exe");

            if (!File.Exists(dlpPath))
            {
                Console.WriteLine("Unable to find yt-dlp.exe.");
                Console.WriteLine($"File should be located in \"{dlpPath}\"");
                Environment.Exit(1);
            }

            string ffmpegPath = Path.Combine(directory.FullName, "Dependencies", "ffmpeg.exe");

            if (!File.Exists(ffmpegPath))
            {
                Console.WriteLine("Unable to find ffmpeg.exe.");
                Console.WriteLine($"File should be located in \"{ffmpegPath}\"");
                Environment.Exit(1);
            }

            return (dlpPath, ffmpegPath);
        }
    }
}