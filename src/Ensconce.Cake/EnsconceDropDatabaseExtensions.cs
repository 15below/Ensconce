using Cake.Core;
using Cake.Core.Annotations;
using Cake.Core.IO;
using Ensconce.Database;
using Ensconce.Update;
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
            context.DropDatabaseByConnectionString(connectionString, string.Empty);
        }

        [CakeMethodAlias]
        public static void DropDatabaseByConnectionString(this ICakeContext context, string connectionString, string fixedStructureFile)
        {
            var tagDictionary = TagDictionaryBuilder.Build(fixedStructureFile);
            var connectionStringBuilder = new SqlConnectionStringBuilder(connectionString.RenderTemplate(tagDictionary));
            context.DropDatabase(connectionStringBuilder);
        }

        [CakeMethodAlias]
        public static void DropLocalDatabaseByName(this ICakeContext context, string databaseName)
        {
            context.DropLocalDatabaseByName(databaseName, string.Empty);
        }

        [CakeMethodAlias]
        public static void DropLocalDatabaseByName(this ICakeContext context, string databaseName, string fixedStructureFile)
        {
            var tagDictionary = TagDictionaryBuilder.Build(fixedStructureFile);
            var connectionStringBuilder = Database.Database.GetLocalConnectionStringFromDatabaseName(databaseName.RenderTemplate(tagDictionary));
            context.DropDatabase(connectionStringBuilder);
        }

        private static void DropDatabase(this ICakeContext context, SqlConnectionStringBuilder sqlConnectionStringBuilder)
        {
            var tempDirectoryPath = new DirectoryPath(Path.Combine(Path.GetTempPath(), Path.GetRandomFileName()));
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

                var database = new Database.Database(sqlConnectionStringBuilder, new LegacyFolderStructure())
                {
                    OutputPath = @"C:\Temp\RoundhousE\"
                };

                database.Deploy(tempDirectoryPath.FullPath, string.Empty, true);
            }
            finally
            {
                tempDirectory.Delete(true);
            }
        }
    }
}
