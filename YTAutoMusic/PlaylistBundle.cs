using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YTAutoMusic
{
    internal class PlaylistBundle
    {
        static readonly string LIST_URL = "?list=";

        public PlaylistBundle()
        {
            string folder;

            Console.WriteLine("Give root path. Insert nothing to put in music directory.");
            folder = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(folder))
            {
                folder = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);
            }

            Console.WriteLine("Give YT playlist URL.");
            string url = Console.ReadLine();

            if(string.IsNullOrWhiteSpace(url))
            {
                throw new FormatException("Must be a playlist url");
            }

            var tempDirectory = Directory.CreateDirectory(folder + @"\temp");
            var baseDirectory = tempDirectory.Parent;


            // get playlist ID
            int listIndex = url.IndexOf(LIST_URL) + LIST_URL.Length;

            if(listIndex == -1)
            {
                throw new FormatException("Must be a playlist url");
            }

            string playlistID = url[listIndex..];

            //

            Environment.Exit(1);

            try
            {
                var ytDLP = new Process();
                ytDLP.StartInfo.FileName = Environment.ExpandEnvironmentVariables(@"%PROGRAMFILES%\yt-dlp\yt-dlp.exe");
                ytDLP.StartInfo.Arguments = $"\"{url}\" -P \"{tempDirectory}\" -f bestaudio --no-overwrites --yes-playlist --no-write-comments --write-description --write-playlist-metafiles";
                ytDLP.Start();

                ytDLP.WaitForExit();

                ytDLP.Dispose();

                Console.WriteLine(tempDirectory);

                var files = tempDirectory.EnumerateFiles();
                var matches = files.Where(f => f.Name.Contains($"[{playlistID}]"));
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error has occured!");
                Console.WriteLine(ex.ToString());
                Environment.Exit(1);
            }
        }
    }
}
