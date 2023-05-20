namespace YTAutoMusic
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine();
            Console.WriteLine("------------------------");
            Console.WriteLine("YouTube Music Downloader");
            Console.WriteLine("------------------------");
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
                Console.WriteLine("What do you want to do?\n'n' - new playlist | 'a' - append playlist | 'q' - quit");
                response = Console.ReadLine();

                switch (response)
                {
                    case "q":
                        Environment.Exit(0);
                        break;
                    case "n":
                        PlaylistBundle.Create(dlpPath, ffmpegPath);
                        break;
                    case "a":
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