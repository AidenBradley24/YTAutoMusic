using System.Reflection;

namespace YTAutoMusic
{
    /// <summary>
    /// Responsible for all metadata filling. Attempts all filling schemes with inherited <see cref="MetadataBase"/>
    /// </summary>
    internal class MetadataFiller
    {
        private readonly IEnumerable<MetadataBase> fillers;

        public MetadataFiller()
        {
            fillers = GetAllFillers();
        }

        public void Fill(MusicBundle bundle, TagLib.File tagFile)
        {
            bool success = false;
            foreach(MetadataBase filler in fillers)
            {
                try
                {
                    if(filler.Fill(tagFile, bundle.Title, bundle.Description))
                    {
                        Console.WriteLine($"Filled metadata with {filler.Name}");
                        success = true;
                        break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"{filler.Name} failed.\n{ex.Message}");
                }
            }

            if(!success)
            {
                tagFile.Tag.Title = bundle.Title;
                tagFile.Tag.Album = "";
                Console.WriteLine($"Unable to fill metadata for '{bundle.Title}'");
            }
        }

        private static IEnumerable<MetadataBase> GetAllFillers()
        {
            List<MetadataBase> fillers = new();
            IEnumerable<Type> metadataFillerTypes = Assembly.GetAssembly(typeof(MetadataBase)).GetTypes().
                Where(t => t.IsClass && !t.IsAbstract && t.IsSubclassOf(typeof(MetadataBase)));

            foreach (Type type in metadataFillerTypes)
            {
                fillers.Add((MetadataBase)Activator.CreateInstance(type));
            }

            fillers.Sort();

            return fillers;
        }
    }
}
