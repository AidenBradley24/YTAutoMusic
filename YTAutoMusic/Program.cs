namespace YTAutoMusic
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("YTAutoMusic\n----------\n");
            
            if(args.Length == 0 )
            {
                NoArgument();
            }

        }

        static void NoArgument()
        {
            string response;

            do
            {
                Console.WriteLine("What do you want to do?\n'n' - new playlist | 'a' - append playlist | 'q' - quit");
                response = Console.ReadLine();

                switch (response)
                {
                    case "q":
                        Environment.Exit(0);
                        break;
                    case "n":
                        var playlist = new PlaylistBundle();
                        break;
                    case "a":
                        break;
                }

            } while (true);
        }
    }
}