using System.Diagnostics;
using System.Text;
using TagLib.Id3v2;

namespace YTAutoMusic
{
    internal static class PlaylistDownloader
    {
        static readonly string LIST_URL = "list=";

        public static void Create(string dlpPath, string ffmpegPath)
        {
            string folder;

            DirectoryInfo baseDirectory;

            while (true)
            {
                Console.WriteLine("Provide root path. Insert nothing to put in music directory.");
                folder = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(folder))
                {
                    folder = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);
                    Console.WriteLine(folder);
                }

                try
                {
                    folder = Path.GetFullPath(folder);
                }
                catch
                {
                    Console.WriteLine($"{folder} is not a valid directory.");
                    continue;
                }

                baseDirectory = Directory.CreateDirectory(folder);

                if (Directory.CreateDirectory(Directory.GetCurrentDirectory()) == baseDirectory)
                {
                    Console.WriteLine("Cannot open here.");
                    continue;
                }

                break;
            }

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

            var tempDirectory = Directory.CreateDirectory(folder + @"\temp");

            // use yt-dlp to download from youtube

            var ytDLP = new Process();
            ytDLP.StartInfo.FileName = dlpPath;
            ytDLP.StartInfo.Arguments = $"\"{url}\" -P \"{tempDirectory}\" -f bestaudio --force-overwrites --yes-playlist --no-write-comments --write-description --write-playlist-metafiles";
            ytDLP.Start();
            ytDLP.WaitForExit();
            ytDLP.Dispose();

            //

            var tempFiles = new TempFileBundle(tempDirectory);

            PlaylistBundle playlistBundle = GetPlaylistInfo(url, tempFiles);

            Console.WriteLine($"Creating Playlist \"{playlistBundle.Name}\"");
            var finalDirectory = Directory.CreateDirectory(baseDirectory + @$"\{playlistBundle.Name}\tracks");      

            FormatAndPlaceAudio(playlistBundle, tempFiles, finalDirectory, ffmpegPath);
            tempFiles.Dispose();

            CreatePlaylistFile(playlistBundle, finalDirectory);

            Console.WriteLine("\n\nPlaylist creation complete.\n\n");
        }

        public static void Append(string dlpPath, string ffmpegPath)
        {
            string folder;
            DirectoryInfo playlistDirectory;

            while (true)
            {
                Console.WriteLine("Provide playlist folder. (should have xspf file and a 'tracks' folder inside)");
                folder = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(folder))
                {
                    continue;
                }

                try
                {
                    folder = Path.GetFullPath(folder);
                }
                catch
                {
                    Console.WriteLine($"{folder} is not a valid directory.");
                    continue;
                }

                if (!Directory.Exists(folder + @"\tracks"))
                {
                    Console.WriteLine("Playlist folder does not have a 'tracks' folder inside.");
                    continue;
                }

                playlistDirectory = Directory.CreateDirectory(folder);

                if (Directory.CreateDirectory(Directory.GetCurrentDirectory()) == playlistDirectory)
                {
                    Console.WriteLine("Cannot open here.");
                    continue;
                }

                break;
            }

            string url;
            while (true)
            {
                Console.WriteLine("Provide YT playlist URL.");
                url = Console.ReadLine();

                if (!string.IsNullOrWhiteSpace(url))
                {
                    break;
                }
            }

            var trackDirectory = Directory.CreateDirectory(playlistDirectory.FullName + @"\tracks");

            var tracks = trackDirectory.EnumerateFiles();

            List<string> ids = new();
            StringBuilder idFilter = new("--match-filter ");
            foreach (var track in tracks)
            {
                var tagFile = TagLib.File.Create(track.FullName);

                Tag idTag = (Tag)tagFile.GetTag(TagLib.TagTypes.Id3v2); // for reading the youtube id
                PrivateFrame p = PrivateFrame.Get(idTag, "yt-id", false);

                string id = Encoding.Unicode.GetString(p.PrivateData.Data);
                ids.Add(id);
                idFilter.Append("id!=");
                idFilter.Append(id);
                idFilter.Append('&');
            }

            string filter = idFilter.ToString()[..^1];

            // use yt-dlp to download from youtube
            var tempDirectory = Directory.CreateDirectory(playlistDirectory.FullName + @"\temp");

            var ytDLP = new Process();
            ytDLP.StartInfo.FileName = dlpPath;
            ytDLP.StartInfo.Arguments = $"\"{url}\" -P \"{tempDirectory}\" -f bestaudio --force-overwrites --yes-playlist --no-write-comments --write-description --write-playlist-metafiles " + filter;
            ytDLP.Start();
            ytDLP.WaitForExit();
            ytDLP.Dispose();

            TempFileBundle tempFiles = new(tempDirectory);
            PlaylistBundle playlistBundle = GetPlaylistInfo(url, tempFiles);

            FormatAndPlaceAudio(playlistBundle, tempFiles, trackDirectory, ffmpegPath);
            tempFiles.Dispose();

            CreatePlaylistFile(playlistBundle, trackDirectory);

            Console.WriteLine("\n\nAppend Playlist Complete\n\n");
        }

        private static readonly string ORIGINAL_DESCRIPTION_TAG = "\n--- ORIGINAL DESCRIPTION ---\n";

        private static void FormatAndPlaceAudio(PlaylistBundle playlist, TempFileBundle tempFiles, DirectoryInfo finalDirectory, string ffmpegPath)
        {
            var ffmpeg = new Process();
            ffmpeg.StartInfo.FileName = ffmpegPath;

            List<MusicBundle> bundles = new(tempFiles.AudioFiles.Count());
            long dataLength = 0L;
            TimeSpan totalDuration = TimeSpan.Zero;

            foreach (var file in finalDirectory.EnumerateFiles())
            {
                var tagFile = TagLib.File.Create(file.FullName);

                Console.WriteLine(file.FullName);
                Console.WriteLine("Grabbing existing file.");

                Tag idTag = (Tag)tagFile.GetTag(TagLib.TagTypes.Id3v2); // for reading the youtube id
                PrivateFrame p = PrivateFrame.Get(idTag, "yt-id", false);

                string id;
                if (p != null)
                {
                    id = Encoding.Unicode.GetString(p.PrivateData.Data);
                }
                else
                {
                    id = "";   
                }

                var bundle = new MusicBundle(file, id, file.Name[..^".mp3".Length]);
                string description = tagFile.Tag.Description;
                string originalDescription = description[(description.IndexOf(ORIGINAL_DESCRIPTION_TAG) + ORIGINAL_DESCRIPTION_TAG.Length)..];

                bundle.Auto(tagFile, originalDescription, playlist);

                var duration = tagFile.Properties.Duration;
                totalDuration += duration;

                tagFile.Tag.Length = duration.ToString(@"hh\:mm\:ss");

                bundles.Add(bundle);

                tagFile.Save();
                tagFile.Dispose();
            }

            foreach (var sound in tempFiles.AudioFiles)
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

                var matches = tempFiles.DescriptionFiles.Where(f => f.Name.Contains($"[{bundle.ID}]"));

                string description = $"Created from a YouTube music playlist.\n" +
                $"https://youtube.com/watch?v={bundle.ID}\n" +
                $"https://youtube.com/playlist?list={playlist.ID}";

                string originalDescription = "";

                if (!matches.Any())
                {
                    Console.WriteLine($"No description found for \"{bundle.Title}\" giving defaults.");
                }
                else
                {
                    using var stream = matches.First().OpenText();
                    originalDescription = stream.ReadToEnd();
                    if (!string.IsNullOrEmpty(originalDescription))
                    {
                        description += ORIGINAL_DESCRIPTION_TAG + originalDescription;
                    }
                }

                bundle.Auto(file, originalDescription, playlist);
                file.Tag.Description = description;

                var duration = file.Properties.Duration;
                totalDuration += duration;

                file.Tag.Length = duration.ToString(@"hh\:mm\:ss");

                Tag idTag = (Tag)file.GetTag(TagLib.TagTypes.Id3v2); // for saving the youtube id
                PrivateFrame p = PrivateFrame.Get(idTag, "yt-id", true);
                p.PrivateData = Encoding.Unicode.GetBytes(bundle.ID);

                file.Save();
                file.Dispose();
            }

            using (StreamWriter writer = new(finalDirectory.Parent.FullName + @"\description.txt"))
            {
                writer.WriteLine(playlist.Name);
                writer.WriteLine("\n------------------\n");
                writer.WriteLine($"Playlist sourced from https://www.youtube.com/playlist?list={playlist.ID}");
                writer.WriteLine("\n------------------\n");
                writer.WriteLine(playlist.Description);
                writer.WriteLine("\n------------------\n");
                writer.WriteLine("Stats:");
                writer.WriteLine($"Track count: {bundles.Count}");
                writer.WriteLine($"File size: {dataLength / 1000} KB");
                writer.WriteLine($"Playlist duration: {totalDuration:hh\\:mm\\:ss}");
            }
        }

        private static string GetNameWithoutURLTag(string name)
        {
            return name[..name.LastIndexOf('[')].Trim();
        }

        private static string GetURLTag(string name)
        {
            return name[(name.LastIndexOf('[') + 1)..name.LastIndexOf(']')];
        }

        private static PlaylistBundle GetPlaylistInfo(string playlistURL, TempFileBundle tempFiles)
        {
            string playlistName, playlistDescription;

            string playlistID;
            if (playlistURL.Contains('&'))
            {
                playlistID = playlistURL.Split('&').Where(s => s.StartsWith(LIST_URL)).First()[LIST_URL.Length..];
            }
            else
            {
                int listIndex = playlistURL.IndexOf(LIST_URL) + LIST_URL.Length;
                playlistID = playlistURL[listIndex..];
            }

            var matches = tempFiles.DescriptionFiles.Where(f => f.Name.Contains($"[{playlistID}]"));

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

            return new PlaylistBundle(playlistName, playlistDescription, playlistID);
        }

        private static void CreatePlaylistFile(PlaylistBundle playlistBundle, DirectoryInfo trackDirectory)
        {
            XspfBuilder gen = new()
            {
                PlaylistBundle = playlistBundle,
                TrackDirectory = trackDirectory,
            };

            gen.Build(trackDirectory.Parent.FullName);
        }

        private class TempFileBundle
        {
            private readonly IEnumerable<FileInfo> descriptionFiles;
            private readonly IEnumerable<FileInfo> audioFiles;
            private readonly IEnumerable<FileInfo> allFiles;

            private bool dead = false;

            public TempFileBundle(DirectoryInfo tempDir)
            {
                allFiles = tempDir.EnumerateFiles();
                descriptionFiles = allFiles.Where(f => f.Name.EndsWith(".description"));
                audioFiles = allFiles.Where(f => !f.Name.EndsWith(".description"));
            }

            public IEnumerable<FileInfo> AudioFiles
            {
                get { if(dead) throw new ObjectDisposedException("tempfiles"); return audioFiles; }
            }

            public IEnumerable<FileInfo> AllFiles
            {
                get { if (dead) throw new ObjectDisposedException("tempfiles"); return allFiles; }
            }

            public IEnumerable<FileInfo> DescriptionFiles
            {
                get { if (dead) throw new ObjectDisposedException("tempfiles"); return descriptionFiles; }
            }

            public void Dispose()
            {
                allFiles.First().Directory.Delete(true);
                dead = true;
            }
        }
    }
}
