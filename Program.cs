using System.Security.Cryptography;
using System.Text;

namespace FolderSynchronizer
{
    class Program
    {
        static void Main(string[] arg)
        {
            MD5 md5 = MD5.Create();
            if (arg.Length < 2)
            {
                Console.WriteLine("Please provide correct arguments");
                return;
            }
            string logFilePath = Path.Combine(Directory.GetCurrentDirectory(), "logs");
            
            string logFileName = "log_" + DateTime.Now.ToString("yyyy-MM-dd_HH:mm:ss") + ".txt";
            logFilePath = Path.Combine(logFilePath, logFileName);


            var logFile = File.Create(logFilePath);
            logFile.Close();


            string sourcePath = arg[0];
            string replicaPath = arg[1];
            var lastTimeCharacter = arg[2][arg[2].Length - 1];

            if (!(lastTimeCharacter > 47 && lastTimeCharacter < 58))
            {
                arg[2] = arg[2].Remove(arg[2].Length - 1);
            }
            else
            {
                lastTimeCharacter = 's';
            }

            float timeInterval = Int32.Parse(arg[2]);

            if (lastTimeCharacter == 's')
            {
                timeInterval *= 1000;
            }
            else if (lastTimeCharacter == 'm')
            {
                timeInterval = timeInterval * 60000;
            }
            else if (lastTimeCharacter == 'h')
            {
                timeInterval = timeInterval * 3600000;
            }
            else
            {
                Console.WriteLine("Please provide correct time interval. Example: 10s, 5m, 1h");
                return;
            }


            if (File.Exists(arg[0]) || File.Exists(arg[1]))
            {
                string logMessage = "\nSource and replica path has to be to directory.";
                Console.Write(logMessage);
                File.AppendAllText(logFilePath, logMessage);
                return;
            }

            if(!Directory.Exists(sourcePath))
            {
                string logMessage = "\nSource path does not exist.";
                Console.Write(logMessage);
                File.AppendAllText(logFilePath, logMessage);
                return;
            }


            if (!Directory.Exists(replicaPath))
            {
                string logMessage = "\nReplica path does not exist.";
                Console.Write(logMessage);
                File.AppendAllText(logFilePath, logMessage);
                return;
            }

            while (true)
            {
                string[] sourceFiles = Directory.GetFiles(sourcePath);
                string[] replicaFiles = Directory.GetFiles(replicaPath);
                var replicaFilesToDelete = new List<String>(replicaFiles);



                foreach (string sourceFilePath in sourceFiles)
                {
                    string sourceFile = Path.GetFileName(sourceFilePath);
                    bool existsInReplica = false;
                    foreach (string replicaFilePath in replicaFiles)
                    {
                        string replicaFile = Path.GetFileName(replicaFilePath);
                        if (sourceFile == replicaFile)
                        {
                            using (var sourceStream = File.OpenRead(sourceFilePath))
                            using (var replicaStream = File.OpenRead(replicaFilePath))
                            {
                                byte[] sourceHash = md5.ComputeHash(sourceStream);
                                byte[] replicaHash = md5.ComputeHash(replicaStream);
                                sourceStream.Close();
                                replicaStream.Close();

                                var sb = new StringBuilder();
                                for (int i = 0; i < sourceHash.Length; i++)
                                {
                                    sb.Append(sourceHash[i].ToString("x2"));
                                }
                                var rb = new StringBuilder();
                                for (int i = 0; i < replicaHash.Length; i++)
                                {
                                    rb.Append(replicaHash[i].ToString("x2"));
                                }

                                if (sb.ToString() != rb.ToString())
                                {
                                    string logMessage = $"\nFile {sourceFile} was changed.";
                                    Console.Write(logMessage);
                                    File.AppendAllText(logFilePath, logMessage);
                                    File.Copy(sourceFilePath, Path.Combine(replicaPath, sourceFile), true);
                                }
                            }
                            existsInReplica = true;
                            replicaFilesToDelete.Remove(replicaFilePath);
                        }
                    }
                    if (!existsInReplica)
                    {
                        string logMessage = $"\nFile {sourceFile} was added.";
                        Console.Write(logMessage);
                        File.AppendAllText(logFilePath, logMessage);
                        File.Copy(sourceFilePath, Path.Combine(replicaPath, sourceFile), true);
                    }
                }

                foreach (string replicaFilePath in replicaFilesToDelete)
                {
                    string replicaFile = Path.GetFileName(replicaFilePath);
                    string logMessage = $"\nFile {replicaFile} was deleted.";
                    Console.Write(logMessage);
                    File.AppendAllText(logFilePath, logMessage);
                    File.Delete(replicaFilePath);
                }

                DateTime now = DateTime.Now;
                string logMessageStandard = $"\nSynchronization completed at {now:yyyy-MM-dd HH:mm:ss}";
                Console.Write(logMessageStandard);
                File.AppendAllText(logFilePath, logMessageStandard);

                System.Threading.Thread.Sleep((int)timeInterval);
            }
        }

    }
}