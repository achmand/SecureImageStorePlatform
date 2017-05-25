using Common;

namespace DataAccess.Repositories
{
    public sealed class LogsRepository : BaseRepository, IBasicRepository<LogException, int?>
    {
        public int? Add(LogException logException)
        {
            if (logException == null)
            {
                return null;
            }

            StoreDbEntities.LogExceptions.Add(logException);
            SaveDatabase();
            return logException.LogId;
        }
    }
}
