using System.Text;
using System.Text.Encodings.Web;

namespace YTAutoMusic.Playlist_Files
{
    internal class M3UFile : PlaylistFile
    {
        public override string ConfigName => "m3u playlist";

        public override void Build(DirectoryInfo targetDirectory, DirectoryInfo trackDirectory, PlaylistBundle bundle)
        {
            using FileStream stream = File.Open(Path.Combine(targetDirectory.FullName, "playlist.m3u"), FileMode.Create);
            using StreamWriter writer = new(stream, Encoding.ASCII);
            writer.WriteLine("#EXTM3U");
            var url = UrlEncoder.Default;

            foreach (FileInfo file in trackDirectory.EnumerateFiles())
            {
                string location = "file:///tracks/" + url.Encode($"{file.Name}");

                TagLib.File tagFile = TagLib.File.Create(file.FullName);
                string artist = string.Join(" & ", tagFile.Tag.Performers).Replace(",", "").Replace("-", "|").Trim();
                string title = tagFile.Tag.Title.Replace(",", "").Replace("-", "|").Trim();

                writer.WriteLine($"#EXTINF:{tagFile.Properties.Duration.Seconds},{artist} - {title}");
                writer.WriteLine(location);
            }
        }
    }
}
