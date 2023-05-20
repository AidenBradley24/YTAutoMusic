using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace YTAutoMusic
{
    internal static class PlaylistBundle
    {
        static readonly string LIST_URL = "?list=";

        public static void Create(string dlpPath, string ffmpegPath)
        {
            string folder;

            Console.WriteLine("Give root path. Insert nothing to put in music directory.");
            folder = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(folder))
            {
                folder = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);
            }

            var tempDirectory = Directory.CreateDirectory(folder + @"\temp");
            var baseDirectory = tempDirectory.Parent;

            string url;
            int listIndex;

            while (true)
            {
                Console.WriteLine("\nProvide a YouTube playlist URL:");
                url = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(url))
                {
                    Console.WriteLine("\nInvalid URL\n");
                    continue;
                }

                listIndex = url.IndexOf(LIST_URL) + LIST_URL.Length;

                if (listIndex == -1)
                {
                    Console.WriteLine("\nInvalid URL\n");
                    continue;
                }

                break;
            }

            string playlistID = url[listIndex..];

            // use yt-dlp to download from youtube

            var ytDLP = new Process();
            ytDLP.StartInfo.FileName = dlpPath;
            ytDLP.StartInfo.Arguments = $"\"{url}\" -P \"{tempDirectory}\" -f bestaudio --no-overwrites --yes-playlist --no-write-comments --write-description --write-playlist-metafiles";
            ytDLP.Start();
            ytDLP.WaitForExit();
            ytDLP.Dispose();

            var files = tempDirectory.EnumerateFiles();
            var descriptionFiles = files.Where(f => f.Name.EndsWith(".description"));
            var soundFiles = files.Where(f => !f.Name.EndsWith(".description"));
            var matches = descriptionFiles.Where(f => f.Name.Contains($"[{playlistID}]"));

            string playlistName;

            if (!matches.Any())
            {
                Console.WriteLine("No name found giving default name.");
                playlistName = $"Playlist [{playlistID}]";
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
            ffmpeg.StartInfo.FileName = ffmpegPath;

            List<(FileInfo, string, string)> newSound = new(soundFiles.Count());
            foreach (var sound in soundFiles)
            {
                string originalName = sound.FullName;
                string rawName = GetNameWithoutURLTag(sound.Name);
                string newName = finalDirectory + @$"\{rawName}.mp3";

                Console.WriteLine($"\n\nConverting to mp3: '{originalName}'\n" +
                    $"\\/\\/\\/\\/\\/\\/\\/\\/\\/\\/\\/\\/\n" +
                    $"'{newName}'\n");

                ffmpeg.StartInfo.Arguments = $"-i \"{originalName}\" \"{newName}\"";
                ffmpeg.Start();
                ffmpeg.WaitForExit();

                sound.Delete(); // delete original file

                FileInfo newFile = new(newName);
                newSound.Add((newFile, GetURLTag(sound.Name), rawName));
            }

            foreach (var bundle in newSound)
            {
                (FileInfo sound, string id, string name) = bundle;

                Console.WriteLine();
                Console.WriteLine($"\n{sound.FullName}");

                Console.WriteLine($"Formatting '{name}' ; ID: '{id}'");

                var file = TagLib.File.Create(sound.FullName);
                file.Tag.DateTagged = DateTime.Now;

                // youtube description

                matches = descriptionFiles.Where(f => f.Name.Contains($"[{id}]"));

                string description = $"Created from a YouTube video." +
                $"URL: youtube.com/watch?v={id}\n";

                if (!matches.Any())
                {
                    Console.WriteLine($"No description found for \"{name}\" giving defaults.");
                }
                else
                {
                    using (var stream = matches.First().OpenText())
                    {
                        string addition = stream.ReadToEnd();
                        if (!string.IsNullOrEmpty(addition))
                        {
                            description += $"\n--- ORIGINAL DESCRIPTION ---\n" + addition;
                        }
                    }
                }

                file.Tag.Description = description;
                file.Tag.Album = playlistName;
                file.Tag.Title = name;

                file.Save();
            }

            tempDirectory.Delete(true);
        }

        private static string GetNameWithoutURLTag(string name)
        {
            return name[..name.LastIndexOf('[')].Trim();
        }

        private static string GetURLTag(string name)
        {
            return name[(name.LastIndexOf('[') + 1)..name.LastIndexOf(']')];
        }
    }
}
