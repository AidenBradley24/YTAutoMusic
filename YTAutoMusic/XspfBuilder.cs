using System.Text;
using System.Text.Encodings.Web;
using System.Xml.Linq;
using TagLib.Id3v2;

namespace YTAutoMusic
{
    internal class XspfBuilder
    {
        public PlaylistBundle PlaylistBundle { get; set; }
        public DirectoryInfo TrackDirectory { get; set; }

        public bool IsValid()
        {
            return PlaylistBundle != null && TrackDirectory != null;
        }

        public void Build(string path)
        {
            if (!IsValid())
            {
                throw new InvalidOperationException("XSPF builder missing information");
            }

            string header = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>" +
               "<playlist xmlns=\"http://xspf.org/ns/0/\" xmlns:vlc=\"http://www.videolan.org/vlc/playlist/ns/0/\" version=\"1\">" +
               "</playlist>";
            XDocument doc = XDocument.Parse(header);

            XElement? playlist = doc.FirstNode as XElement;

            XNamespace ns = playlist.GetDefaultNamespace();
            XNamespace vlcNS = playlist.GetNamespaceOfPrefix("vlc");

            XElement tracklist = new(ns + "trackList");

            playlist.Add(new object[] 
            {
                new XElement(ns + "title", PlaylistBundle.Name),
                new XElement(ns + "extension", new object[] 
                {
                    new XAttribute("application", "http://www.videolan.org/vlc/playlist/0"),
                    new XElement(vlcNS + "item",
                       new XAttribute("tid", 0)
                    )
                }),
                new XElement(ns + "info", $"https://www.youtube.com/playlist?list={PlaylistBundle.ID}"),
                tracklist,
            });

            int counter = 0;
            var url = UrlEncoder.Default;

            foreach (FileInfo file in TrackDirectory.EnumerateFiles())
            {
                string location = "file:///tracks/" + url.Encode($"{file.Name}");

                XElement extension = new(ns + "extension");

                extension.Add(new object[] 
                {
                    new XAttribute("application", "http://www.videolan.org/vlc/playlist/0"),
                    new XElement(vlcNS + "id", counter),
                });

                TagLib.File tagFile = TagLib.File.Create(file.FullName);

                Tag idTag = (Tag)tagFile.GetTag(TagLib.TagTypes.Id3v2); // for reading the youtube id
                PrivateFrame p = PrivateFrame.Get(idTag, "yt-id", false);

                string info;
                if(p != null)
                {
                    string id = Encoding.Unicode.GetString(p.PrivateData.Data);
                    info = $"https://www.youtube.com/watch?v={id}&list={PlaylistBundle.ID}";
                }
                else
                {
                    info = $"Added to playlist without YouTube";
                }

                tracklist.Add(new object[] 
                {
                   new XElement(ns + "track", new object[] 
                   {
                       new XElement(ns + "location", location),
                       new XElement(ns + "title", tagFile.Tag.Title),
                       new XElement(ns + "info", info),
                       extension
                   })
                });

                counter++;
            }

            using FileStream stream = new(Path.Combine(path, "playlist.xspf"), FileMode.Create);
            doc.Save(stream);
        }
    }
}
