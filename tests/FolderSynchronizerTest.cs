using Xunit;

namespace FolderSynchronizer.Tests
{
    public class FolderSynchronizerTests : IDisposable
    {

        private readonly string testDirection;
        private readonly string sourceDirection;
        private readonly string replicaDirection;
        private readonly string sourceFilePath;
        private readonly string replicaFilePath;
        private readonly ILogger logger;
        private readonly FolderSynchronizer folderSynchronizer;

        public FolderSynchronizerTests()
        {
            // ARRANGE (wspólne dla wszystkich testów)
            // Tworzymy unikalny folder dla każdego uruchomienia testów
            testDirection = Path.Combine(Path.Combine(Directory.GetCurrentDirectory(), "TestData"));
            sourceDirection = Path.Combine(testDirection, "TestSource");
            replicaDirection = Path.Combine(testDirection, "TestReplica");
            sourceFilePath = Path.Combine(sourceDirection, "TestFile.txt");
            replicaFilePath = Path.Combine(replicaDirection, "TestFile.txt");

            Directory.CreateDirectory(testDirection);
            Directory.CreateDirectory(sourceDirection);
            Directory.CreateDirectory(replicaDirection);

            logger = new Logger(Path.Combine(testDirection, "test_log.txt"));
            folderSynchronizer = new FolderSynchronizer(sourceDirection, replicaDirection, logger);
        }

        [Fact]
        public void Copy_Newly_Added_File()
        {
            var file = File.Create(sourceFilePath);
            file.Close();
            folderSynchronizer.Synchronize();

            Assert.True(File.Exists(replicaFilePath));
        }

        [Fact]
        public void Update_Changed_File()
        {
            var file = File.Create(sourceFilePath);
            file.Close();
            folderSynchronizer.Synchronize();
            File.Copy(sourceFilePath, replicaFilePath, true);

            File.AppendAllText(sourceFilePath, "Some new content");
            folderSynchronizer.Synchronize();
            Assert.Equal(File.ReadAllText(sourceFilePath), File.ReadAllText(replicaFilePath));
        }

        [Fact]
        public void Delete_Removed_File()
        {
            var file = File.Create(sourceFilePath);
            file.Close();
            folderSynchronizer.Synchronize();
            Assert.True(File.Exists(replicaFilePath));

            File.Delete(sourceFilePath);
            folderSynchronizer.Synchronize();
            Assert.False(File.Exists(replicaFilePath));
        }


        public void Dispose()
        {
            if (Directory.Exists(testDirection))
            {
                Directory.Delete(testDirection, true);
            }
        }
    }
}
