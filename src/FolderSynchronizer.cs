using System.Security.Cryptography;

namespace FolderSynchronizer
{
    public class FolderSynchronizer
    {
        private string sourcePath;   // Source folder path
        private string replicaPath; // Replica folder path
        private readonly ILogger logger;
        public FolderSynchronizer(string source, string replica, ILogger _logger)
        {
            logger = _logger;
            sourcePath = source;
            replicaPath = replica;
        }

        public void Synchronize()
        {
            try
            {
                string[] sourceFiles = Directory.GetFiles(sourcePath);
                string[] replicaFiles = Directory.GetFiles(replicaPath);
                var replicaFilesToDelete = new List<string>(replicaFiles);
                MD5 md5 = MD5.Create();

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
