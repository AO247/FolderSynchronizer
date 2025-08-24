namespace FolderSynchronizer
{
    class Program
    {
        public static async Task Main(string[] arg)
        {
            if (arg.Length < 4)
            {
                Console.WriteLine("Please provide correct arguments");
                Console.WriteLine("Example: FolderSynchronizer.exe <source path> <replica path> <time interval> <log file path>");
                return;
            }
            
            ILogger logger = new Logger(arg[3]);
            FolderSynchronizer folderSynchronizer = new FolderSynchronizer(arg[0], arg[1], logger);

            float timeInterval = ParseTimeInterval(arg[2], logger);
            if (timeInterval < 0.0f) return; // if time interval is invalid, exit the program

            while (true) // main loop
            {
                folderSynchronizer.Synchronize();
                await Task.Delay((int)timeInterval);
            }
           
        }

        private static float ParseTimeInterval(string arg, ILogger logger)
        {
            var lastTimeCharacter = arg[arg.Length - 1];  // get last character of time interval argument

            if (!char.IsDigit(lastTimeCharacter)) // if last character is not a digit, remove it from the string
            {
                arg = arg.Remove(arg.Length - 1);
            }
            else
            {
                lastTimeCharacter = 's'; // base time interval is seconds
            }
            float timeInterval = 0.0f;
            try
            {
                timeInterval = int.Parse(arg);
            }
            catch (FormatException)
            {
                logger.LogError($"Please provide correct time interval. Example: 10s, 5m, 1h");
                return -1.0f;
            }

            if (timeInterval <= 0)
            {
                logger.LogError($"Please provide positive time interval. Example: 10s, 5m, 1h");
                return -1.0f;
            }

            if (lastTimeCharacter == 's')       // seconds
            {
                timeInterval *= 1000;
            }
            else if (lastTimeCharacter == 'm')  // minutes
            {
                timeInterval = timeInterval * 60000;
            }
            else if (lastTimeCharacter == 'h')  // hours
            {
                timeInterval = timeInterval * 3600000;
            }
            else
            {
                logger.LogError($"Please provide correct time interval. Example: 10s, 5m, 1h");
                return -1.0f;
            }
            return timeInterval;
        }
    }
}