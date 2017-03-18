using System;

namespace DataAccess.Repositories
{
    public abstract class BaseRepository : IDisposable
    {
        internal ImageStoreDBEntities StoreDbEntities { get; set; }

        protected BaseRepository()
        {
            StoreDbEntities = new ImageStoreDBEntities();
        }

        protected void SaveDatabase()
        {
            StoreDbEntities.SaveChanges();
        }

        public void Dispose()
        {
            StoreDbEntities.Dispose();
        }
    }
}
