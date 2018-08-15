using NUnit.Framework;
using Rhino.Mocks;
using roundhouse.infrastructure.logging;
using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Ensconce.Database.Tests
{
    [TestFixture]
    [Explicit("Want to run these against local database")]
    [Category("Integration Test")]
    public class TemporaryDatabaseTests
    {
        [Test]
        public void should_dispose_when_not_deployed()
        {
            var sut = new TemporaryDatabase();
            sut.Dispose();
        }


        [Test]
        public void should_remove_database_when_disposed()
        {
            var sut = new TemporaryDatabase();
            sut.Deploy();
            sut.Dispose();
            Assert.That(sut.Exists(), Is.False);
        }

        [Test]
        public void should_create_tables_and_sprocs_when_deployed()
        {
            using (var sut = new TemporaryDatabase())
            {
                sut.Deploy(Path.Combine(Assembly.GetExecutingAssembly().Directory(), "Scripts1"));
                Assert.That(sut.GetTables().Select(x => x.Name), Has.Member("Table1"));
            }
        }

        [Test]
        public void should_drop_database_when_deployed_with_dropDatabase_option()
        {
            using (var sut = new TemporaryDatabase())
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
            using (var sut = new TemporaryDatabase())
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
            using (var sut = new TemporaryDatabase())
            {
                sut.Deploy(Path.Combine(Assembly.GetExecutingAssembly().Directory(), "Scripts1"));
                Assert.That(sut.ReadVersion(), Is.EqualTo(currentVersion));
            }
        }


        [Test]
        public void should_stamp_repository_path_when_deployed()
        {
            const string repositoryPath = "testrepository";
            using (var sut = new TemporaryDatabase())
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

            using (var sut = new TemporaryDatabase(restoreOptions))
            {
                sut.Deploy(Path.Combine(Assembly.GetExecutingAssembly().Directory(), "Scripts1"));
                Assert.That(sut.GetTables().Select(x => x.Name), Has.Member("Table1"));
                Assert.That(sut.GetTables().Select(x => x.Name), Has.Member("Table2"));
            }
        }

        [Test]
        public void should_not_display_fluent_nhibernate_warnings()
        {
            // Get around problem with calling roundhouse dll directly where it continually logs missing type for nhibernate
            var logger = MockRepository.GenerateMock<Logger>();

            using (var sut = new TemporaryDatabase(null, logger))
            {
                sut.Deploy();

                var args = logger.GetArgumentsForCallsMadeOn(x => x.log_a_warning_event_containing(null, null));

                Assert.That(args[0], Has.None.ContainsSubstring("Had an error building session factory from merged, attempting unmerged. The error:"));
            }
        }


    }
}
