using static YTAutoMusic.MetadataFillerExtensions;

namespace YTAutoMusic.Metadata_Fillers
{
    public class SoundtrackMetadata : MetadataBase
    {
        public override int Priority => 3;

        public override string Name => "'soundtrack' config";

        public override bool Fill(TagLib.File tagFile, string title, string description)
        {
            if (IsStandaloneWord("OST", title, out string usedWord) || IsStandaloneWord("O.S.T.", title, out usedWord) || IsStandaloneWord("Soundtrack", title, out usedWord))
            {
                var bits = title.Split(SEPERATORS, StringSplitOptions.RemoveEmptyEntries);

                int i;
                for (i = 0; i < bits.Length; i++)
                {
                    if (IsStandaloneWord(usedWord, bits[i], out _))
                    {
                        break;
                    }
                }

                int soundtrackIndex = i;

                if (i < bits.Length)
                {
                    string album = bits[i].Trim().Trim(CLEAN_UP_TRIM);
                    int blacklist = -1;

                    if (album == usedWord)
                    {
                        i--;
                        if (i < 0)
                        {
                            throw new IndexOutOfRangeException("Can't find soundtrack name");
                        }

                        blacklist = i;

                        album = bits[i].Trim();
                        if (album.Length == 0)
                        {
                            throw new FormatException("Can't find soundtrack name");
                        }
                    }

                    if (usedWord == "O.S.T.")
                    {
                        album = album.Replace("O.S.T.", "OST");
                    }

                    string a = "";

                    foreach (string word in album.Split(' '))
                    {
                        string trimmedWord = word.Trim(CLEAN_UP_TRIM);

                        if (trimmedWord.Equals("OST", StringComparison.InvariantCultureIgnoreCase) ||
                            trimmedWord.Equals("Soundtrack", StringComparison.InvariantCultureIgnoreCase))
                        {
                            break;
                        }

                        a += word + " ";
                    }

                    tagFile.Tag.Album = a + "Soundtrack";

                    i++;

                    string t = null;

                    for (; i < bits.Length; i++)
                    {
                        if (i == blacklist) continue;

                        string bit = bits[i].Trim(' ', '\n', '\t', '0', '1', '2', '3', '4', '5', '6', '7', '8', '9');
                        if (string.IsNullOrWhiteSpace(bit))
                        {
                            continue;
                        }

                        t = bit;
                    }

                    i = soundtrackIndex - 1;

                    if (t == null)
                    {
                        for (; i >= 0; i--)
                        {
                            if (i == blacklist) continue;

                            string bit = bits[i].Trim(' ', '\n', '\t', '0', '1', '2', '3', '4', '5', '6', '7', '8', '9');
                            if (string.IsNullOrWhiteSpace(bit))
                            {
                                continue;
                            }

                            t = bit;
                        }
                    }

                    if (t == null)
                    {
                        throw new FormatException("Title failed");
                    }

                    t = t.Trim(CLEAN_UP_TRIM);

                    tagFile.Tag.Title = t;
                }

                tagFile.Tag.Genres = new string[] { "Soundtrack" };

                return true;
            }

            return false;
        }
    }
}
