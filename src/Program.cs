using System.Security.Cryptography;

namespace FolderSynchronizer
{
    class Program
    {
        static void Main(string[] arg)
        {
            if (arg.Length < 4)
            {
                Console.WriteLine("Please provide correct arguments");
                Console.WriteLine("Example: FolderSynchronizer.exe <source path> <replica path> <time interval> <log file path>");
                return;
            }

            MD5 md5 = MD5.Create();
            Logger logger = new Logger(arg[3]);


            string sourcePath = arg[0];                         // get source path from arguments
            string replicaPath = arg[1];                        // get replica path from arguments
            var lastTimeCharacter = arg[2][arg[2].Length - 1];  // get last character of time interval argument

            if (!(lastTimeCharacter > 47 && lastTimeCharacter < 58)) // if last character is not a digit, remove it from the string
            {
                arg[2] = arg[2].Remove(arg[2].Length - 1);
            }
            else
            {
                lastTimeCharacter = 's'; // base time interval is seconds
            }

            float timeInterval = int.Parse(arg[2]);

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
                return;
            }


            try
            {
                while (true) // main loop
                {
                    string[] sourceFiles = Directory.GetFiles(sourcePath);
                    string[] replicaFiles = Directory.GetFiles(replicaPath);
                    var replicaFilesToDelete = new List<string>(replicaFiles);

                    foreach (string sourceFilePath in sourceFiles)              
                    {
                        string sourceFile = Path.GetFileName(sourceFilePath);
                        bool existsInReplica = false;
                        foreach (string replicaFilePath in replicaFiles)
                        {
                            string replicaFile = Path.GetFileName(replicaFilePath);
                            if (sourceFile == replicaFile)                                  // detect if file exists in replica and source folder
                            {
                                using (var sourceStream = File.OpenRead(sourceFilePath))
                                using (var replicaStream = File.OpenRead(replicaFilePath))
                                {
                                    byte[] sourceHash = md5.ComputeHash(sourceStream);
                                    byte[] replicaHash = md5.ComputeHash(replicaStream);
                                    sourceStream.Close();
                                    replicaStream.Close();

                                    if (!sourceHash.SequenceEqual(replicaHash))             // if file hashes are different, copy source file to replica (overwrite)
                                    {
                                        logger.Log($"File {sourceFile} was changed.");

                                        File.Copy(sourceFilePath, Path.Combine(replicaPath, sourceFile), true);
                                    }
                                }
                                existsInReplica = true;
                                replicaFilesToDelete.Remove(replicaFilePath);
                            }
                        }
                        if (!existsInReplica)                                               // if file does not exist in replica, copy it there
                        {
                            logger.Log($"File {sourceFile} was added.");

                            File.Copy(sourceFilePath, Path.Combine(replicaPath, sourceFile), true);
                        }
                    }

                    foreach (string replicaFilePath in replicaFilesToDelete)                // delete files from replica that do not exist in source
                    {
                        string replicaFile = Path.GetFileName(replicaFilePath);
                        logger.Log($"File {replicaFile} was deleted.");

                        File.Delete(replicaFilePath);
                    }

                    Thread.Sleep((int)timeInterval);
                }
            }
            catch (UnauthorizedAccessException uaEx)
            {
                logger.LogError($"Access error: {uaEx.Message}");
            }
            catch (IOException ioEx)
            {
                logger.LogError($"I/O error: {ioEx.Message}");
            }
            catch (Exception ex)
            {
                logger.LogError($"An error occurred: {ex.Message}");
            }
        }
    }
}