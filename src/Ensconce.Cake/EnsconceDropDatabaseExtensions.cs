using Cake.Core;
using Cake.Core.Annotations;
using Cake.Core.IO;
using Ensconce.Database;
using Ensconce.Update;
using FifteenBelow.Octopus.Cake;
using System.Data.SqlClient;
using System.IO;
using Path = System.IO.Path;

namespace Ensconce.Cake
{
    public static class EnsconceDropDatabaseExtensions
    {
        [CakeMethodAlias]
        public static void DropDatabaseByConnectionString(this ICakeContext context, string connectionString)
        {
            context.DropDatabaseByConnectionString(connectionString, null, null);
        }

        [CakeMethodAlias]
        public static void DropDatabaseByConnectionString(this ICakeContext context, string connectionString, DirectoryPath tempDirectoryPath)
        {
            context.DropDatabaseByConnectionString(connectionString, null, tempDirectoryPath);
        }

        [CakeMethodAlias]
        public static void DropDatabaseByConnectionString(this ICakeContext context, string connectionString, FilePath fixedStructureFile, DirectoryPath tempDirectoryPath)
        {
            var tagDictionary = fixedStructureFile == null ? TagDictionaryBuilder.Build(string.Empty) : TagDictionaryBuilder.Build(fixedStructureFile.FullPath);
            var connectionStringBuilder = new SqlConnectionStringBuilder(connectionString.RenderTemplate(tagDictionary));
            context.DropDatabase(connectionStringBuilder, tempDirectoryPath);
        }

        [CakeMethodAlias]
        public static void DropLocalDatabaseByName(this ICakeContext context, string databaseName)
        {
            context.DropLocalDatabaseByName(databaseName, null, null);
        }

        [CakeMethodAlias]
        public static void DropLocalDatabaseByName(this ICakeContext context, string databaseName, DirectoryPath tempDirectoryPath)
        {
            context.DropLocalDatabaseByName(databaseName, null, tempDirectoryPath);
        }

        [CakeMethodAlias]
        public static void DropLocalDatabaseByName(this ICakeContext context, string databaseName, FilePath fixedStructureFile, DirectoryPath tempDirectoryPath)
        {
            var tagDictionary = fixedStructureFile == null ? TagDictionaryBuilder.Build(string.Empty) : TagDictionaryBuilder.Build(fixedStructureFile.FullPath);
            var connectionStringBuilder = Database.Database.GetLocalConnectionStringFromDatabaseName(databaseName.RenderTemplate(tagDictionary));
            context.DropDatabase(connectionStringBuilder, tempDirectoryPath);
        }

        private static void DropDatabase(this ICakeContext context, SqlConnectionStringBuilder sqlConnectionStringBuilder, DirectoryPath tempDirectoryPath)
        {
            var deleteDir = false;
            DirectoryPath outputPath;
            if (tempDirectoryPath == null)
            {
                tempDirectoryPath = new DirectoryPath(Path.Combine(Path.GetTempPath(), Path.GetRandomFileName()));
                outputPath = new DirectoryPath(@"C:\Temp\RoundhousE\");
                deleteDir = true;
            }
            else
            {
                outputPath = tempDirectoryPath.Combine(new DirectoryPath("RoundhousE"));
            }

            context.LogDebug($"Using RounhousE output path {outputPath}");

            var tempDirectory = context.FileSystem.GetDirectory(tempDirectoryPath);

            if (tempDirectory.Exists)
            {
                tempDirectory.Delete(true);
            }
            tempDirectory.Create();

            var buildInfoFilePath = tempDirectory.Path.CombineWithFilePath(new FilePath("_BuildInfo.txt"));
            var buildInfoFile = context.FileSystem.GetFile(buildInfoFilePath);

            if (buildInfoFile.Exists)
            {
                buildInfoFile.Delete();
            }

            try
            {
                using (var fileStream = buildInfoFile.OpenWrite())
                using (var fileStreamWriter = new StreamWriter(fileStream))
                {
                    fileStreamWriter.WriteLine("Drop-DB");
                }

                var database = new Database.Database(sqlConnectionStringBuilder)
                {
                    OutputPath = outputPath.FullPath
                };

                database.Deploy(tempDirectoryPath.FullPath, dropDatabase: true);
                context.LogInfo($"Dropped '{sqlConnectionStringBuilder.InitialCatalog}' on server '{sqlConnectionStringBuilder.DataSource}'");
            }
            finally
            {
                if (deleteDir)
                {
                    tempDirectory.Delete(true);
                }
            }
        }
    }
}
