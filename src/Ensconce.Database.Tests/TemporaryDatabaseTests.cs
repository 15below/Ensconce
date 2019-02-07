using NUnit.Framework;
using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Ensconce.Database.Tests
{
    [TestFixture]
    [Category("Integration Test")]
    public class TemporaryDatabaseTests
    {
        private TemporaryDatabase GetTemporaryDatabase(DatabaseRestoreOptions restoreOptions = null)
        {
            var dbUser = Environment.GetEnvironmentVariable("DbUser");
            var dbPass = Environment.GetEnvironmentVariable("DbPass");

            if (!string.IsNullOrWhiteSpace(dbUser) && !string.IsNullOrWhiteSpace(dbPass))
            {
                return new TemporaryDatabase(restoreOptions, null, dbUser, dbPass);
            }

            return new TemporaryDatabase(restoreOptions, null);
        }

        [Test]
        public void should_dispose_when_not_deployed()
        {
            var sut = GetTemporaryDatabase();
            sut.Dispose();
        }

        [Test]
        public void should_remove_database_when_disposed()
        {
            var sut = GetTemporaryDatabase();
            sut.Deploy();
            sut.Dispose();
            Assert.That(sut.Exists(), Is.False);
        }

        [Test]
        public void should_create_tables_and_sprocs_when_deployed()
        {
            using (var sut = GetTemporaryDatabase())
            {
                sut.Deploy(Path.Combine(Assembly.GetExecutingAssembly().Directory(), "Scripts1"));
                Assert.That(sut.GetTables().Select(x => x.Name), Has.Member("Table1"));
            }
        }

        [Test]
        public void should_drop_database_when_deployed_with_dropDatabase_option()
        {
            using (var sut = GetTemporaryDatabase())
            {
                sut.Deploy(string.Empty, string.Empty, true);
                Assert.True(!sut.Exists());
            }
        }

        [Test]
        public void should_stamp_version_from_default_buildinfo_file_when_deployed()
        {
            // This version number is set in _BuildInfo.txt which lives at the root of the database scripts
            const string currentVersion = "1.1.1.1";
            using (var sut = GetTemporaryDatabase())
            {
                sut.Deploy(Path.Combine(Assembly.GetExecutingAssembly().Directory(), "Scripts1"));
                Assert.That(sut.ReadVersion(), Is.EqualTo(currentVersion));
            }
        }

        [Test]
        [Ignore("TODO: Allow db deploy to detect version from Environment variable and use this instead, if present. This would be useful for Octopus Deploy and would remove the necessity for a version file to be present for tag replacement")]
        public void should_stamp_version_from_environment_variable_when_deployed()
        {
            // This version number is set in _BuildInfo.txt which lives at the root of the database scripts
            const string currentVersion = "1.1.1.2";
            Environment.SetEnvironmentVariable("PackageVersion", currentVersion);
            using (var sut = GetTemporaryDatabase())
            {
                sut.Deploy(Path.Combine(Assembly.GetExecutingAssembly().Directory(), "Scripts1"));
                Assert.That(sut.ReadVersion(), Is.EqualTo(currentVersion));
            }
        }

        [Test]
        public void should_stamp_repository_path_when_deployed()
        {
            const string repositoryPath = "testrepository";
            using (var sut = GetTemporaryDatabase())
            {
                sut.Deploy(Path.Combine(Assembly.GetExecutingAssembly().Directory(), "Scripts1"), repositoryPath);
                Assert.That(sut.ReadRepository(), Is.EqualTo(repositoryPath));
            }
        }

        [Test]
        public void should_run_restore_from_backup_and_then_deploy()
        {
            var restoreOptions =
                new DatabaseRestoreOptions(Path.Combine(Assembly.GetExecutingAssembly().Directory(), "ensconcedb.bak"));

            using (var sut = GetTemporaryDatabase(restoreOptions))
            {
                sut.Deploy(Path.Combine(Assembly.GetExecutingAssembly().Directory(), "Scripts1"));
                Assert.That(sut.GetTables().Select(x => x.Name), Has.Member("Table1"));
                Assert.That(sut.GetTables().Select(x => x.Name), Has.Member("Table2"));
            }
        }
    }
}
