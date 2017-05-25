using System;
using System.Linq;
using System.Linq.Expressions;
using Common;

namespace DataAccess.Repositories
{
    public sealed class ImagesRepository : BaseRepository, IBasicRepository<Image, string>
    {
        #region properties and variables

        public IQueryable<Image> AllImages => StoreDbEntities.Images;
        
        #endregion
        
        #region public methods

        public string Add(Image image)
        {
            if (image == null)
            {
                return null;
            }

            StoreDbEntities.Images.Add(image);
            StoreDbEntities.SaveChanges();
            return image.ImagePath;
        }

        public Image GetByIdentifier(Func<Image, bool> where)
        {
            return StoreDbEntities.Images.SingleOrDefault(where);
        }

        public IQueryable<Image> Find(Expression<Func<Image, bool>> where)
        {
            return StoreDbEntities.Images.Where(where);
        }

        #endregion

    }
}
