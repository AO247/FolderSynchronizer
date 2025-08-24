namespace FolderSynchronizer
{
    public class Logger
    {
        private readonly string logPath;        // Main log file path
        private readonly string errorLogPath;   // Error log file path

        public Logger(string logFilePath) // Initialize logger
        {
            logPath = logFilePath;
            errorLogPath = Path.Combine(
                Path.GetDirectoryName(logPath) ?? ".",
                Path.GetFileNameWithoutExtension(logPath) + "_error" + Path.GetExtension(logPath));
            
        }

        public void Log(string message) // Log general messages
        {
            try
            {
                string msg = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}";

                Console.WriteLine(msg);

                using (var writer = new StreamWriter(logPath, true))
                {
                    writer.WriteLine(msg);
                }
            }
            catch (Exception ex)
            {
                LogError($"Failed to write to log file: {ex.Message}");
            }
        }

        public void LogError(string errorMessage) // Log errors to a separate error log file
        {
            try
            {
                string msg = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] ERROR: {errorMessage}";

                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(msg);
                Console.ResetColor();

                using (var writer = new StreamWriter(errorLogPath, true))
                {
                    writer.WriteLine(msg);
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Failed to write to error log file: {ex.Message}");
                Console.ResetColor();
            }
        }
    }
}
