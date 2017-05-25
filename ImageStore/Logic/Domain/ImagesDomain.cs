using System;
using System.Linq;
using Common;
using Common.Utilities;
using DataAccess.Repositories;
using Logic.DomainObjects;

namespace Logic.Domain
{
    public sealed class ImagesDomain : IDisposable
    {
        #region properties & variables 

        private ImagesRepository ImagesRepository { get; }

        #endregion

        #region ctors 

        public ImagesDomain()
        {
            ImagesRepository = new ImagesRepository();
        }

        #endregion

        #region public methods 

        public DomainResult<string> Add(string imagePath, string signature, string username, string title)
        {
            var domainResult = new DomainResult<string>();

            var verify = VerifyInputs(imagePath, signature, username, title);
            if (verify)
            {
                var userExist = new UsersDomain().UsernameExists(username);
                if (!userExist)
                {
                    domainResult.ProcessResult = ProcessResult.Failure;
                    domainResult.MessageResult = "Username does not exist";
                }

                var image = new Image
                {
                    DateCreated = HomelessMethods.GetCurrentTime(),
                    Version = 1,
                    ImagePath = imagePath,
                    Signature = signature,
                    Title = title,
                    UsernameFk = username
                };

                var path = ImagesRepository.Add(image);
                domainResult.MessageResult = "Success";
                domainResult.ProcessResult = ProcessResult.Success;
                domainResult.ObjectResult = path;
                return domainResult;
            }

            domainResult.MessageResult = "Not all inputs are correct";
            domainResult.ProcessResult = ProcessResult.Failure;
            return domainResult;
        }

        public IQueryable<Image> GetImages()
        {
            var images = ImagesRepository.AllImages;
            return images;
        }

        public Image GetImage(int imageId)
        {
            var image = ImagesRepository.GetByIdentifier(i => i.ImageId == imageId);
            return image;
        }

        public IQueryable<Image> ImagesUser(string username)
        {
            var images = ImagesRepository.Find(i => i.UsernameFk == username);
            return images;
        }

        public void Dispose()
        {
            ImagesRepository.Dispose();
        }

        #endregion

        #region private methods 

        private static bool VerifyInputs(string imagePath, string signature, string username, string title)
        {
            return (!string.IsNullOrEmpty(imagePath) || !string.IsNullOrEmpty(username) ||
                    !string.IsNullOrEmpty(signature) || !string.IsNullOrEmpty(title));
        }

        #endregion

    }
}
