namespace FifteenBelow.Deployment
{
    public interface IDatabase
    {
        void Deploy(string schemaScriptsFolder, string repository, bool dropDatabase);
    }
}