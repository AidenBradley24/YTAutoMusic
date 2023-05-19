using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TagLib;

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
                var descriptionFiles = files.Where(f => f.Name.EndsWith(".description"));
                var soundFiles = files.Where(f => !f.Name.EndsWith(".description"));

                var matches = descriptionFiles.Where(f => f.Name.Contains($"[{playlistID}]"));
                
                string playlistName;
                if(!matches.Any())
                {
                    Console.WriteLine("No name found giving default name.");
                    playlistName = "Playlist #" + playlistID;
                }
                else
                {
                    var file = matches.First();

                    playlistName = GetNameWithoutURLTag(file.Name);
                    var invalids = Path.GetInvalidFileNameChars();
                    playlistName = string.Join("_", playlistName.Split(invalids, StringSplitOptions.RemoveEmptyEntries)).TrimEnd('.');
                }

                Console.WriteLine($"Creating Playlist \"{playlistName}\"");
                var finalDirectory = Directory.CreateDirectory(baseDirectory + @$"\{playlistName}");

                var ffmpeg = new Process();
                ffmpeg.StartInfo.FileName = Environment.ExpandEnvironmentVariables(@"%PROGRAMFILES%\ffmpeg\ffmpeg.exe");         

                List<FileInfo> newSound = new List<FileInfo>(soundFiles.Count());
                foreach(var sound in soundFiles)
                {
                    ffmpeg.StartInfo.Arguments = $"-i {sound.FullName} {finalDirectory + @$"\{sound.Name[..sound.Name.LastIndexOf('.')]}.mp3"}";
                    ffmpeg.Start();
                    ffmpeg.WaitForExit();
                }

                foreach (var sound in newSound)
                {
                    Console.WriteLine(sound.FullName);

                    var rawName = sound.Name[..sound.Name.LastIndexOf('.')];

                    matches = descriptionFiles.Where(f => f.Name[..f.Name.LastIndexOf('.')] == rawName);
                    var id = GetURLTag(rawName);

                    Console.WriteLine($"Formatting '{rawName}' ; ID: '{id}'");

                    if (!matches.Any())
                    {
                        Console.WriteLine($"No name found for \"{rawName}\" giving defaults.");
                        playlistName = $"Playlist [{playlistID}]";

                        var file = TagLib.File.Create(sound.FullName);
                        file.Tag.DateTagged = DateTime.Now;

                        string description = $"Created from a YouTube video." +
                            $"URL: youtube.com/watch?v={id}\n";

                        using (var stream = descriptionFiles.Where(f => f.Name.Contains($"[{id}]")).First().OpenText())
                        {
                            string addition = stream.ReadToEnd();
                            if (!string.IsNullOrEmpty(addition))
                            {
                                description += $"\n--- ORIGINAL DESCRIPTION ---\n" + addition;
                            }
                        }

                        file.Tag.Description = description;
                        file.Tag.Album = playlistName;
                        file.Tag.Title = GetNameWithoutURLTag(sound.Name);
                    }
                    else
                    {
                        var file = matches.First();
                        string tempName = file.Name;
                        playlistName = tempName[..tempName.LastIndexOf('[')];
                        var invalids = Path.GetInvalidFileNameChars();
                        playlistName = string.Join("_", playlistName.Split(invalids, StringSplitOptions.RemoveEmptyEntries)).TrimEnd('.');
                    }
                }

                tempDirectory.Delete(true);
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error has occured!");
                Console.WriteLine(ex.ToString());
                Environment.Exit(1);
                tempDirectory.Delete(true);
            }
        }

        private static string GetNameWithoutURLTag(string name)
        {
            return name[..name.LastIndexOf('[')].Trim();
        }

        private static string GetURLTag(string name)
        {
            return name[(name.LastIndexOf('[')+1)..name.LastIndexOf(']')];
        }
    }
}
