using Cake.Core;
using Cake.Core.Annotations;
using Cake.Core.IO;
using Ensconce.Database;
using Ensconce.Update;
using System.Data.SqlClient;
using System.IO;

namespace Ensconce.Cake
{
    public static class EnsconceDeployDatabaseExtensions
    {
        [CakeMethodAlias]
        public static void DeployDatabaseByConnectionString(this ICakeContext context, string connectionString, DirectoryPath deployScriptDirectoryPath)
        {
            context.DeployDatabaseByConnectionString(connectionString, null, deployScriptDirectoryPath);
        }

        [CakeMethodAlias]
        public static void DeployDatabaseByConnectionString(this ICakeContext context, string connectionString, FilePath fixedStructureFile, DirectoryPath deployScriptDirectoryPath)
        {
            var tagDictionary = fixedStructureFile == null ? TagDictionaryBuilder.Build(string.Empty) : TagDictionaryBuilder.Build(fixedStructureFile.FullPath);
            var connectionStringBuilder = new SqlConnectionStringBuilder(connectionString.RenderTemplate(tagDictionary));
            context.DeployDatabase(connectionStringBuilder, deployScriptDirectoryPath);
        }

        [CakeMethodAlias]
        public static void DeployLocalDatabaseByName(this ICakeContext context, string databaseName, DirectoryPath deployScriptDirectoryPath)
        {
            context.DeployLocalDatabaseByName(databaseName, null, deployScriptDirectoryPath);
        }

        [CakeMethodAlias]
        public static void DeployLocalDatabaseByName(this ICakeContext context, string databaseName, FilePath fixedStructureFile, DirectoryPath deployScriptDirectoryPath)
        {
            var tagDictionary = fixedStructureFile == null ? TagDictionaryBuilder.Build(string.Empty) : TagDictionaryBuilder.Build(fixedStructureFile.FullPath);
            var connectionStringBuilder = Database.Database.GetLocalConnectionStringFromDatabaseName(databaseName.RenderTemplate(tagDictionary));
            context.DeployDatabase(connectionStringBuilder, deployScriptDirectoryPath);
        }

        private static void DeployDatabase(this ICakeContext context, SqlConnectionStringBuilder sqlConnectionStringBuilder, DirectoryPath deployScriptDirectoryPath)
        {
            var deployScriptDirectory = context.FileSystem.GetDirectory(deployScriptDirectoryPath);

            if (!deployScriptDirectory.Exists)
            {
                throw new DirectoryNotFoundException($"Unable to locate {deployScriptDirectoryPath.FullPath}");
            }

            var database = new Database.Database(sqlConnectionStringBuilder, new LegacyFolderStructure())
            {
                OutputPath = deployScriptDirectoryPath.FullPath
            };

            database.Deploy(deployScriptDirectoryPath.FullPath);
        }
    }
}
