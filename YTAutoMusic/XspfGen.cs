using System.Text.Encodings.Web;
using System.Xml.Linq;

namespace YTAutoMusic
{
    internal class XspfGen
    {
        private readonly string name;
        private readonly IEnumerable<MusicBundle> bundles;

        public XspfGen(string name, IEnumerable<MusicBundle> bundles)
        {
            this.name = name;
            this.bundles = bundles;
        }

        public void Build(string path)
        {
            string header = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>" +
               "<playlist xmlns=\"http://xspf.org/ns/0/\">" +
               "</playlist>";
            XDocument doc = XDocument.Parse(header);

            XElement? playlist = doc.FirstNode as XElement;
            XNamespace ns = playlist.GetDefaultNamespace();

            XElement tracklist = new(ns + "tracklist");

            playlist.Add(new object[] {
                new XElement(ns + "title", name),
                tracklist,
            });

            var url = UrlEncoder.Default;
            foreach (MusicBundle bundle in bundles)
            {
                string location = "file:///tracks/" + url.Encode($"{bundle.File.Name}");

                tracklist.Add(new object[] {
                   new XElement(ns + "track", new object[] {
                       new XElement(ns + "location", location),
                       new XElement(ns + "title", bundle.Title),
                       new XElement(ns + "album", name),
                       new XElement(ns + "duration", bundle.Length),
                   })
                });
            }

            using FileStream stream = new(path + @"\playlist.xspf", FileMode.Create);
            doc.Save(stream);
        }
    }
}
