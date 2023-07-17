using static YTAutoMusic.MetadataFillerExtensions;

namespace YTAutoMusic.Metadata_Fillers
{
    internal class MusicKeywordMetadata : MetadataBase
    {
        public override int Priority => 4;

        public override string Name => "'music description' config";

        public override bool Fill(TagLib.File tagFile, string title, string description)
        {
            if (IsWord("Music ", description, out string usedWord) || IsWord("Title ", description, out usedWord))
            {
                var lines = LineifyDescription(description).AsEnumerable();

                var possibleLines = lines.Where(l => l.StartsWith(usedWord, StringComparison.InvariantCultureIgnoreCase));
                if (!possibleLines.Any())
                {
                    throw new FormatException("Not real 'music description'");
                }

                string titleLine = possibleLines.First();
                titleLine = titleLine[usedWord.Length..];

                (string t, string a) = SplitFirst(titleLine);

                t = t.Trim(CLEAN_UP_TRIM);
                a = a.Trim(CLEAN_UP_TRIM);

                if (!title.Contains(t))
                {
                    throw new FormatException("Not real 'music description'");
                }

                if (string.IsNullOrWhiteSpace(a))
                {
                    tagFile.Tag.Title = t;
                    tagFile.Tag.Album = "";
                }
                else
                {
                    tagFile.Tag.Title = t;
                    tagFile.Tag.Album = a;
                }

                return true;
            }

            return false;
        }
    }
}
