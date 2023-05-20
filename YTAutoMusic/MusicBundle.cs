namespace YTAutoMusic
{
    internal class MusicBundle
    {
        public string Title { get; private set; }
        public FileInfo File { get; private set; }
        public string ID { get; private set; }

        public long Length { get; set; }

        public MusicBundle(FileInfo file, string id, string title)
        {
            File = file;
            ID = id;
            Title = title;
        }
    }
}
