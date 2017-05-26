using System;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using Common;
using Logic.Domain;
using Logic.Security;
using WebApp.Attributes;
using WebApp.Filters;
using WebApp.Models;

namespace WebApp.Controllers
{
    [Authorize]
    public sealed class ImagesController : Controller
    {
        #region properties and variables 

        private UsersDomain _usersDomain;
        private ImagesDomain _imagesDomain;

        public UsersDomain UsersDomain
        {
            get
            {
                return _usersDomain ?? new UsersDomain();
            }
            private set
            {
                _usersDomain = value;
            }
        }

        public ImagesDomain ImagesDomain
        {
            get
            {
                return _imagesDomain ?? new ImagesDomain();
            }
            private set
            {
                _imagesDomain = value;
            }
        }

        private string Username => Encryption.DecClaim(HttpContext.User.Identity.Name);

        #endregion

        #region ctors

        public ImagesController()
        {
        }

        public ImagesController(UsersDomain usersDomain, ImagesDomain imagesDomain)
        {
            UsersDomain = usersDomain;
            ImagesDomain = imagesDomain;
        }

        #endregion

        #region actions 

        [HttpGet]
        [ClaimsAuthorize("Permission", "UploadImg")]
        public ActionResult Upload()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ClaimsAuthorize("Permission", "UploadImg")]
        public ActionResult Upload(HttpPostedFileBase file, string title)
        {
            try
            {
                if (string.IsNullOrEmpty(title))
                {
                    return View();
                }

                var fileBytes = ReadFile(file.InputStream);
                var typeCheck = MimeSniffer.GetMime(fileBytes);
                var fileType = typeCheck.Substring(typeCheck.LastIndexOf("/", StringComparison.Ordinal) + 1);
                var checkFile = VerifyFileType(fileType);

                if (checkFile)
                {
                    file.InputStream.Position = 0;
                    var extension = Path.GetExtension(file.FileName);
                    var filename = Guid.NewGuid() + extension;
                    var absolutePath = Server.MapPath(@"\Images") + @"\" + filename;
                    var relativePath = @"\Images\" + filename;

                    var username = Username;
                    var user = UsersDomain.GetUser(username);
                    if (user == null)
                    {
                        return View();
                    }

                    using (var memoryStrem = new MemoryStream())
                    {
                        file.InputStream.CopyTo(memoryStrem);
                        memoryStrem.Position = 0;
                        file.InputStream.Position = 0;

                        var image = memoryStrem.ToArray();
                        var encryption = new Encryption();
                        var signatureBytes = encryption.SignFile(image, user);

                        var result = ImagesDomain.Add(relativePath, Convert.ToBase64String(signatureBytes), username, title);
                        file.SaveAs(absolutePath);
                        var cipher = encryption.HybridEncrypt(file.InputStream, user);
                        System.IO.File.WriteAllBytes(absolutePath, cipher);
                        ViewBag.Message = result.MessageResult;
                    }
                }
            }

            catch (Exception e)
            {
                LogsDomain.LogLevel("Images Domain", e.Message);
                return RedirectToAction("InternalServer", "Error");
            }

            return View();
        }

        [HttpGet]
        [EncryptedActionParameter]
        [ClaimsAuthorize("Permission", "ViewImages")]
        public ActionResult ViewImage(int imageId)
        {
            try
            {
                var username = Username;
                var image = ImagesDomain.GetImage(imageId);

                var identity = (ClaimsIdentity)Thread.CurrentPrincipal.Identity;
                var roleEnc = identity.Claims.SingleOrDefault(c => c.Type == ClaimTypes.Role);
                if (roleEnc == null)
                {
                    return RedirectToAction("ViewImages");
                }

                var roleDec = Encryption.DecClaim(roleEnc.Value);
                if (image.UsernameFk != username && roleDec == "Default")
                {
                    return RedirectToAction("ViewImages");
                }

                string relativePath;
                var decryptedBytes = DecryptImage(image, image.UsernameFk, out relativePath);
                if (decryptedBytes == null)
                {
                    return RedirectToAction("ViewImages");
                }

                var imageModel = new ImageViewModel
                {
                    Title = image.Title,
                    Image = decryptedBytes
                };

                return View(imageModel);
            }

            catch (Exception e)
            {
                LogsDomain.LogLevel("Images Domain", e.Message);
                return RedirectToAction("InternalServer", "Error");
            }
        }

        [HttpGet]
        [ClaimsAuthorize("Permission", "ViewImages")]
        public ActionResult ViewImages()
        {
            try
            {
                var username = Username;
                var images = ImagesDomain.ImagesUser(username);
                return View(images);

            }
            catch (Exception e)
            {
                LogsDomain.LogLevel("Images Domain", e.Message);
                return RedirectToAction("InternalServer", "Error");
            }
        }

        [HttpGet]
        [ClaimsAuthorize("Permission", "ViewAllImages")]
        public ActionResult AdminImages()
        {
            try
            {
                var images = ImagesDomain.GetImages();
                return View(images);
            }
            catch (Exception e)
            {
                LogsDomain.LogLevel("Images Domain", e.Message);
                return RedirectToAction("InternalServer", "Error");
            }
        }

        [HttpGet]
        [EncryptedActionParameter]
        [ClaimsAuthorize("Permission", "DownloadImgs")]
        public ActionResult DownloadImage(int imageId)
        {
            try
            {
                var username = Username;
                var image = ImagesDomain.GetImage(imageId);
                if (image == null)
                {
                    return RedirectToAction("ViewImages");
                }
                if (image.UsernameFk != username)
                {
                    return RedirectToAction("ViewImages");
                }

                string relativePath;
                var decryptedBytes = DecryptImage(image, username, out relativePath);
                if (decryptedBytes == null)
                {
                    return RedirectToAction("ViewImages");
                }

                return File(decryptedBytes, System.Net.Mime.MediaTypeNames.Application.Octet, Path.GetFileName(relativePath));
            }
            catch (Exception e)
            {
                LogsDomain.LogLevel("Images Domain", e.Message);
                return RedirectToAction("InternalServer", "Error");
            }
        }

        #endregion

        #region private methods 

        private static byte[] ReadFile(Stream input)
        {
            var buffer = new byte[16 * 1024];
            using (var ms = new MemoryStream())
            {
                int read;
                while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }

                return ms.ToArray();
            }
        }
        private static bool VerifyFileType(string filetype)
        {
            return filetype == "gif" || filetype == "png" || filetype == "jpeg";
        }
        private byte[] DecryptImage(Image image, string username, out string relativePath)
        {
            relativePath = image.ImagePath;
            var absolutePath = Server.MapPath(relativePath);
            var fileAsCipher = System.IO.File.ReadAllBytes(absolutePath);

            using (var memoryStream = new MemoryStream(fileAsCipher))
            {
                var user = UsersDomain.GetUser(username);
                memoryStream.Position = 0;
                var encryption = new Encryption();
                var decryptedBytes = encryption.HybridDecrypt(memoryStream, user);
                var signature = Convert.FromBase64String(image.Signature);
                var checkSignature = encryption.VerifyFile(signature, decryptedBytes, user);

                if (checkSignature)
                {
                    return decryptedBytes;
                }

            }
            return null;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_usersDomain != null)
                {
                    _usersDomain.Dispose();
                    _usersDomain = null;
                }

                if (_imagesDomain != null)
                {
                    _imagesDomain.Dispose();
                    _imagesDomain = null;
                }
            }

            base.Dispose(disposing);
        }

        #endregion
    }
}