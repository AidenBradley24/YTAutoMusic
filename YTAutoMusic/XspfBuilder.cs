using System.Text.Encodings.Web;
using System.Xml.Linq;

namespace YTAutoMusic
{
    internal class XspfBuilder
    {
        private string name;
        private IEnumerable<MusicBundle> bundles;
        private string playlistID;

        internal string Name { get => name; set => name = value; }
        internal IEnumerable<MusicBundle> Bundles { get => bundles; set => bundles = value; }
        internal string PlaylistID { get => playlistID; set => playlistID = value; }

        public XspfBuilder(string name, string playlistID, IEnumerable<MusicBundle> bundles)
        {
            this.name = name;
            this.playlistID = playlistID;
            this.bundles = bundles;
        }

        public bool IsValid()
        {
            return name != null && bundles != null && playlistID != null;
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

            playlist.Add(new object[] {
                new XElement(ns + "title", name),
                new XElement(ns + "extension", new object[] {
                    new XAttribute("application", "http://www.videolan.org/vlc/playlist/0"),
                    new XElement(vlcNS + "item",
                       new XAttribute("tid", 0)
                    )
                }),
                new XElement(ns + "info", $"https://www.youtube.com/playlist?list={playlistID}"),
                tracklist,
            });

            int counter = 0;
            var url = UrlEncoder.Default;
            foreach (MusicBundle bundle in bundles)
            {
                string location = "file:///tracks/" + url.Encode($"{bundle.File.Name}");

                XElement extension = new(ns + "extension");

                extension.Add(new object[] {
                    new XAttribute("application", "http://www.videolan.org/vlc/playlist/0"),
                    new XElement(vlcNS + "id", counter),
                });

                string ytURL = $"https://www.youtube.com/watch?v={bundle.ID}&list={playlistID}";

                tracklist.Add(new object[] {
                   new XElement(ns + "track", new object[] {
                       new XElement(ns + "location", location),
                       new XElement(ns + "title", bundle.Title),
                       new XElement(ns + "info", ytURL),
                       extension
                   })
                });

                counter++;
            }

            using FileStream stream = new(path + @"\playlist.xspf", FileMode.Create);
            doc.Save(stream);
        }
    }
}
