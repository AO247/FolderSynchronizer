using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FolderSynchronizer
{
    public class Logger
    {
        private readonly string logPath;
        private readonly string errorLogPath;

        public Logger(string logFilePath) // Initialize logger
        {
            logPath = logFilePath;
            errorLogPath = Path.Combine(
                Path.GetDirectoryName(logPath),
                Path.GetFileNameWithoutExtension(logPath) + "_error" + Path.GetExtension(logPath));
            Console.WriteLine($"Logging to {logPath} and {errorLogPath}");
        }

        public void Log(string message) // Log general messages to the main log file
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
            catch
            {
            }
        }
    }
}
