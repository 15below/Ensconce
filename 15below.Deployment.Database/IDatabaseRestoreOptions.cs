using roundhouse;

namespace FifteenBelow.Deployment
{
    public interface IDatabaseRestoreOptions
    {
        void SetRunRestoreOptions(Migrate migrateSettings);
    }
}