using System;

namespace Ensconce
{
    public interface IDatabase
    {
        void Deploy(string schemaScriptsFolder, string repository, bool dropDatabase, TimeSpan? commandTimeout);
    }
}
