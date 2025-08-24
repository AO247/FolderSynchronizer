using System.IO;
using System.Security.Cryptography;
using System.Text;

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

            float timeInterval = int.Parse(arg[2]);

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
                logger.LogError($"Please provide correct time interval. Example: 10s, 5m, 1h");
                return;
            }


            if (File.Exists(arg[0]) || File.Exists(arg[1]))
            {
                logger.LogError($"Source and replica path has to be to directory.");
                return;
            }

            if(!Directory.Exists(sourcePath))
            {
                logger.LogError($"Source path does not exist.");
                return;
            }


            if (!Directory.Exists(replicaPath))
            {
                logger.LogError($"Replica path does not exist.");
                return;
            }

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
                                    logger.Log($"File {sourceFile} was changed.");

                                    File.Copy(sourceFilePath, Path.Combine(replicaPath, sourceFile), true);
                                }
                            }
                            existsInReplica = true;
                            replicaFilesToDelete.Remove(replicaFilePath);
                        }
                    }
                    if (!existsInReplica)
                    {
                        logger.Log($"File {sourceFile} was added.");

                        File.Copy(sourceFilePath, Path.Combine(replicaPath, sourceFile), true);
                    }
                }

                foreach (string replicaFilePath in replicaFilesToDelete)
                {
                    string replicaFile = Path.GetFileName(replicaFilePath);
                    logger.Log($"File {replicaFile} was deleted.");

                    File.Delete(replicaFilePath);
                }

                Thread.Sleep((int)timeInterval);
            }
        }

    }
}