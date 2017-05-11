using System.Security.Cryptography;
using System.Text;

namespace WebApi.Security
{
    public sealed class SecurityUtil
    {
        #region Symmetric

        public SymmetricParameters GenerateSymParams(string password)
        {
            var salt = ""; // TODO -> Check how to generate a good salt
            var saltAsByte = Encoding.UTF32.GetBytes(salt);
            var generator= new Rfc2898DeriveBytes(password, saltAsByte);

            var rijndael = Rijndael.Create();
            var symParams = new SymmetricParameters
            {
                SecretKey = generator.GetBytes(rijndael.KeySize / 8),
                Iv = generator.GetBytes(rijndael.BlockSize / 2)
            };

            return symParams;
        }
        

        #endregion
    }
}