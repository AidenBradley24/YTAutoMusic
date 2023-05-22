using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

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
            string playlistDescription;

            if (!matches.Any())
            {
                Console.WriteLine("No name found giving default name.");
                playlistName = $"Playlist [{playlistID}]";
                playlistDescription = "";
            }
            else
            {
                var file = matches.First();

                playlistName = GetNameWithoutURLTag(file.Name);
                var invalids = Path.GetInvalidFileNameChars();
                playlistName = string.Join("_", playlistName.Split(invalids, StringSplitOptions.RemoveEmptyEntries)).TrimEnd('.');

                using var stream = matches.First().OpenText();
                playlistDescription = stream.ReadToEnd();
            }

            Console.WriteLine($"Creating Playlist \"{playlistName}\"");
            var finalDirectory = Directory.CreateDirectory(baseDirectory + @$"\{playlistName}\tracks");

            var ffmpeg = new Process();
            ffmpeg.StartInfo.FileName = ffmpegPath;

            List<MusicBundle> bundles = new(soundFiles.Count());
            long dataLength = 0L;

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

                FileInfo newFile = new(newName);
                var bundle = new MusicBundle(newFile, GetURLTag(sound.Name), rawName);
                dataLength += newFile.Length;
                sound.Delete(); // delete original file
                bundles.Add(bundle);
            }

            foreach (var bundle in bundles)
            {
                Console.WriteLine();
                Console.WriteLine($"\n{bundle.File.FullName}");

                Console.WriteLine($"Formatting '{bundle.Title}' ; ID: '{bundle.ID}'");

                var file = TagLib.File.Create(bundle.File.FullName, TagLib.ReadStyle.Average);
                file.Tag.DateTagged = DateTime.Now;

                // youtube description

                matches = descriptionFiles.Where(f => f.Name.Contains($"[{bundle.ID}]"));

                string description = $"Created from a YouTube video." +
                $"URL: youtube.com/watch?v={bundle.ID}\n";

                if (!matches.Any())
                {
                    Console.WriteLine($"No description found for \"{bundle.Title}\" giving defaults.");
                }
                else
                {
                    using var stream = matches.First().OpenText();
                    string addition = stream.ReadToEnd();
                    if (!string.IsNullOrEmpty(addition))
                    {
                        description += $"\n--- ORIGINAL DESCRIPTION ---\n" + addition;
                    }
                }

                file.Tag.Description = description;
                file.Tag.Album = playlistName;
                file.Tag.Title = bundle.Title;

                file.Save();
                file.Dispose();
            }

            tempDirectory.Delete(true);

            XspfBuilder gen = new(playlistName, playlistID, bundles);
            gen.Build(finalDirectory.Parent.FullName);

            using (StreamWriter writer = new(finalDirectory.Parent.FullName + @"\description.txt"))
            {
                writer.WriteLine(playlistName);
                writer.WriteLine("\n-_-_-_-_-_-_-_-_-_\n");
                writer.WriteLine($"Playlist sourced from https://www.youtube.com/playlist?list={playlistID}");
                writer.WriteLine("\n-_-_-_-_-_-_-_-_-_\n");
                writer.WriteLine(playlistDescription);
                writer.WriteLine("\n-_-_-_-_-_-_-_-_-_\n");
                writer.WriteLine("Stats:");
                writer.WriteLine($"Track count: {bundles.Count}");
                writer.WriteLine($"File size: {dataLength / 1000} KB");
            }

            Console.WriteLine("\n\nPlaylist creation complete.\n\n");
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
