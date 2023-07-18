namespace YTAutoMusic
{
    /// <summary>
    /// A meta data filling scheme; Only one can be used per song.
    /// </summary>
    public abstract class MetadataBase : IComparable<MetadataBase>
    {
        /// <summary>
        /// Fill tag file metadata based on an abstract scheme
        /// </summary>
        /// <param name="tagFile">Tag file to modify</param>
        /// <param name="title">YT video title</param>
        /// <param name="description">YT video description</param>
        /// <returns>True if successful</returns>
        /// <exception cref="Exception">On a failure after the attempt has begun</exception>
        public abstract bool Fill(TagLib.File tagFile, string title, string description);

        /// <summary>
        /// Fillers with a lower value are attempted first
        /// </summary>
        public abstract int Priority { get; }

        /// <summary>
        /// Shown name of filler
        /// </summary>
        public abstract string Name { get; }

        public int CompareTo(MetadataBase other)
        {
            return Priority.CompareTo(other.Priority);
        }
    }
}
