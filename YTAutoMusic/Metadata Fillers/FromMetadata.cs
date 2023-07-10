using static YTAutoMusic.MetadataFillerExtensions;

namespace YTAutoMusic.Metadata_Fillers
{
    internal class FromMetadata : MetadataBase
    {
        public override int Priority => 1;

        public override string Name => "'from' config";

        public override bool Fill(TagLib.File tagFile, string title, string description)
        {
            if (IsStandaloneWord("From", title, out string usedWord))
            {
                if (!title.Contains($"({usedWord}", StringComparison.InvariantCultureIgnoreCase))
                {
                    throw new FormatException("Not real 'from' config");
                }

                string[] bits = title.Split("(", StringSplitOptions.TrimEntries);

                string t = bits[0].Trim();
                string a = bits[1][usedWord.Length..^1];

                foreach (char q in QUOTES)
                {
                    a = a.Replace(q.ToString(), "");
                }

                a = a.Trim();

                tagFile.Tag.Title = t;
                tagFile.Tag.Album = a;

                return true;
            }

            return false;
        }
    }
}
