using System.Diagnostics;

namespace YTAutoMusic
{
    internal class ConversionHandeler
    {
        private readonly int MAX_PROCESS_COUNT;

        private readonly List<MusicBundle> bundles;
        private readonly Queue<string> argumentQueue;

        private readonly string ffmpegPath;

        public ConversionHandeler(IEnumerable<FileInfo> toConvert, string finalDirectory, string ffmpegPath)
        {
            this.ffmpegPath = ffmpegPath;

            MAX_PROCESS_COUNT = Math.Max(Environment.ProcessorCount / 2, 1);

            Console.WriteLine("\nStarting Conversion.");
            Console.WriteLine($"Allowing {MAX_PROCESS_COUNT} processes\n");

            bundles = new(toConvert.Count());
            argumentQueue = new(toConvert.Count());

            foreach (var sound in toConvert)
            {
                string originalName = sound.FullName;
                string rawName = PlaylistDownloader.GetNameWithoutURLTag(sound.Name);
                string newName = finalDirectory + @$"\{rawName}.mp3";

                Console.WriteLine($"Queuing conversion: '{originalName}'\n");

                FileInfo newFile = new(newName);
                var bundle = new MusicBundle(newFile, PlaylistDownloader.GetURLTag(sound.Name), rawName);
                bundles.Add(bundle);

                argumentQueue.Enqueue($"-i \"{originalName}\" \"{newName}\"");
            }

            Console.WriteLine("\n");
        }

        public async Task Convert()
        {
            List<Task> tasks = new(MAX_PROCESS_COUNT);

            while (argumentQueue.Any() || tasks.Any())
            {
                tasks.RemoveAll(task => task.IsCompleted);

                if (tasks.Count < MAX_PROCESS_COUNT && argumentQueue.Any())
                {
                    Task task = ConvertIndividual(argumentQueue.Dequeue());
                    tasks.Add(task); 
                }

                await Task.Delay(100);
            }
        }

        private async Task ConvertIndividual(string args)
        {
            Console.WriteLine($"\nConversion start: '{args}'\n");

            var ffmpeg = new Process()
            {
                StartInfo = new ProcessStartInfo()
                {
                    FileName = ffmpegPath,
                    Arguments = args,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    RedirectStandardInput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                }
            };

            ffmpeg.Start();
            await ffmpeg.WaitForExitAsync();

            Console.WriteLine($"\nConversion complete: '{args}'\n");
        }

        public MusicBundle[] GetMusicBundles()
        {
            return bundles.ToArray();
        }
    }
}
