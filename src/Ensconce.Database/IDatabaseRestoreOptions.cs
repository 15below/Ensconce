using roundhouse;

namespace Ensconce
{
    public interface IDatabaseRestoreOptions
    {
        void SetRunRestoreOptions(Migrate migrateSettings);
    }
}