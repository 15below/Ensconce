using System;

namespace Ensconce.Database
{
    public interface IDatabase
    {
        void Deploy(string schemaScriptsFolder, string repository, bool dropDatabase, TimeSpan? commandTimeout);
    }
}
