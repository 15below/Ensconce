using roundhouse;

namespace Ensconce.Database
{
    public interface IDatabaseRestoreOptions
    {
        void SetRunRestoreOptions(Migrate migrateSettings);
    }
}
