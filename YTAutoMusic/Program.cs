namespace YTAutoMusic
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine();
            Console.WriteLine(Resources.splash);
            Console.WriteLine();

            (string dlpPath, string ffmpegPath) = FindDependencies();

            if (args.Length == 0 )
            {
                NoArgument(dlpPath, ffmpegPath);
            }
        }

        static void NoArgument(string dlpPath, string ffmpegPath)
        {
            string response;

            do
            {
                Console.WriteLine("What do you want to do?\n'n' - new playlist | 'a' - append playlist | 'q' - quit | 'h' - help");
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
            string dlpPath = Environment.ExpandEnvironmentVariables(@"%PROGRAMFILES%\yt-dlp\yt-dlp.exe");

            if (!File.Exists(dlpPath))
            {
                Console.WriteLine("Unable to find yt-dlp.exe.");
                Console.WriteLine($"File should be located in \"{dlpPath}\"");
                Environment.Exit(404);
            }

            string ffmpegPath = Environment.ExpandEnvironmentVariables(@"%PROGRAMFILES%\ffmpeg\ffmpeg.exe");

            if (!File.Exists(ffmpegPath))
            {
                Console.WriteLine("Unable to find ffmpeg.exe.");
                Console.WriteLine($"File should be located in \"{ffmpegPath}\"");
                Environment.Exit(404);
            }

            return (dlpPath, ffmpegPath);
        }
    }
}