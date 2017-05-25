using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Common;

namespace Logic.Security
{
    public sealed class Encryption
    {
        #region symmetric 

        public static SymmetricParameters GenerateSymmetricParameters(User user)
        {
            if (user == null)
            {
                throw new Exception("User cannot be null");
            }

            var username = user.Username;
            var password = user.Password;
            var salt = username + password;

            var saltBytes = Encoding.UTF32.GetBytes(salt);

            using (var rijndael = Rijndael.Create())
            using (var rfc = new Rfc2898DeriveBytes(password, saltBytes))
            {
                var symetricParams = new SymmetricParameters
                {
                    SecretKey = rfc.GetBytes(rijndael.KeySize / 8),
                    IV = rfc.GetBytes(rijndael.BlockSize / 8)
                };

                return symetricParams;
            }
        }

        public string SymmetricEncrypt(string data, User user)
        {
            if (user == null)
            {
                throw new Exception("User cannot be null");
            }

            var symmetricParams = GenerateSymmetricParameters(user);
            var dataBytes = Encoding.UTF32.GetBytes(data);

            using (var memoryStream = new MemoryStream())
            using (var rijndael = Rijndael.Create())
            using (var cryptoStream = new CryptoStream(memoryStream, rijndael.CreateEncryptor(symmetricParams.SecretKey, symmetricParams.IV), CryptoStreamMode.Write))
            {
                cryptoStream.Write(dataBytes, 0, dataBytes.Length);
                cryptoStream.FlushFinalBlock();
                memoryStream.Position = 0;

                var encData = memoryStream.ToArray();
                cryptoStream.Close();
                memoryStream.Close();
                return Convert.ToBase64String(encData);
            }
        }

        public byte[] SymmetricEncryptFile(Stream inputStream, User user)
        {
            if (user == null)
            {
                throw new Exception("User cannot be null");
            }

            var symmetricParams = GenerateSymmetricParameters(user);
            using (var rijndael = Rijndael.Create())
            using (var memoryStream = new MemoryStream())
            using (var cryptoStream = new CryptoStream(memoryStream, rijndael.CreateEncryptor(symmetricParams.SecretKey, symmetricParams.IV), CryptoStreamMode.Write))
            {
                inputStream.CopyTo(cryptoStream);
                cryptoStream.FlushFinalBlock();
                memoryStream.Position = 0;

                return memoryStream.ToArray();
            }
        }

        public byte[] SymmetricDecryptFile(Stream cipher, User user)
        {
            var symmetricParams = GenerateSymmetricParameters(user);
            using (var rijndael = Rijndael.Create())
            using (var memoryStream = new MemoryStream())
            using (var cryptoStream = new CryptoStream(memoryStream, rijndael.CreateDecryptor(symmetricParams.SecretKey, symmetricParams.IV), CryptoStreamMode.Write))
            {
                cipher.CopyTo(cryptoStream);
                cryptoStream.FlushFinalBlock();
                memoryStream.Position = 0;
                return memoryStream.ToArray();
            }
        }

        #endregion

        #region Asymmetric

        public AsymmetricParameters GeneratePublicAndPrivateKey()
        {
            using (var rsa = new RSACryptoServiceProvider())
            {
                var asymmetricParams = new AsymmetricParameters
                {
                    PublicKey = rsa.ToXmlString(false),
                    PrivateKey = rsa.ToXmlString(true)
                };

                return asymmetricParams;
            }
        }

        public byte[] AsymmetricEncrypt(byte[] input, User user)
        {
            var publicKey = user.PublicKey;
            using (var rsa = new RSACryptoServiceProvider())
            {
                rsa.FromXmlString(publicKey);
                var cipher = rsa.Encrypt(input, true);
                return cipher;
            }
        }

        public byte[] AsymmetricDecrypt(byte[] input, User user)
        {
            var privateKey = user.PrivateKey;
            using (var rsa = new RSACryptoServiceProvider())
            {
                rsa.FromXmlString(privateKey);
                var cipher = rsa.Decrypt(input, true);
                return cipher;
            }
        }

        #endregion

        #region digital signing

        public byte[] HashFile(byte[] fileData)
        {
            using (var sha = SHA512.Create())
            {
                var digest = sha.ComputeHash(fileData);
                return digest;
            }
        }

        public byte[] SignFile(byte[] fileData, User user)
        {
            using (var rsa = new RSACryptoServiceProvider())
            {
                var privateKey = user.PrivateKey;
                rsa.FromXmlString(privateKey);

                var fileDigest = HashFile(fileData);
                return rsa.SignData(fileDigest, "SHA512");
            }
        }

        public bool VerifyFile(byte[] signature, byte[] fileData, User user)
        {
            var publicKey = user.PublicKey;
            using (var rsa = new RSACryptoServiceProvider())
            {
                rsa.FromXmlString(publicKey);
                var fileDigest = HashFile(fileData);
                return rsa.VerifyData(fileDigest, "SHA512", signature);
            }
        }

        #endregion

        #region classes

        public class AsymmetricParameters
        {
            public string PublicKey { get; set; }
            public string PrivateKey { get; set; }
        }
        public class SymmetricParameters
        {
            public byte[] SecretKey { get; set; }
            public byte[] IV { get; set; }
        }

        #endregion

        #region encrypt/decrypt query strings 

        public static string EncQueryString(string plainText)
        {
            return EncryptQs(plainText);
        }

        public static string DecQueryString(string encryptedText)
        {
            return DecryptQs(encryptedText);
        }

        private static string DecryptQs(string encryptedText)
        {
            const string key = "jdsg432387#";
            byte[] iv = { 55, 34, 87, 64, 87, 195, 54, 21 };

            var decryptKey = Encoding.UTF8.GetBytes(key.Substring(0, 8));
            encryptedText = encryptedText.Replace(" ", "+");

            using (var des = new DESCryptoServiceProvider())
            using (var ms = new MemoryStream())
            using (var cs = new CryptoStream(ms, des.CreateDecryptor(decryptKey, iv), CryptoStreamMode.Write))
            {
                var inputByte = Convert.FromBase64String(encryptedText);
                cs.Write(inputByte, 0, inputByte.Length);
                cs.FlushFinalBlock();
                var encoding = Encoding.UTF8;
                return encoding.GetString(ms.ToArray());
            }
        }

        private static string EncryptQs(string plainText)
        {
            const string key = "jdsg432387#";
            byte[] iv = { 55, 34, 87, 64, 87, 195, 54, 21 };
            var encryptKey = Encoding.UTF8.GetBytes(key.Substring(0, 8));

            using (var des = new DESCryptoServiceProvider())
            using (var mStream = new MemoryStream())
            using (var cStream = new CryptoStream(mStream, des.CreateEncryptor(encryptKey, iv), CryptoStreamMode.Write))
            {
                var inputByte = Encoding.UTF8.GetBytes(plainText);
                cStream.Write(inputByte, 0, inputByte.Length);
                cStream.FlushFinalBlock();
                return Convert.ToBase64String(mStream.ToArray());
            }
        }

        #endregion

        #region encrypt/decrypt claim info

        public static string EncClaim(string plainText)
        {
            return EncryptClaim(plainText);
        }

        public static string DecClaim(string encryptedText)
        {
            return DecryptClaim(encryptedText);
        }

        private static string DecryptClaim(string cipherText)
        {
            const string encryptionKey = "FFF301A39F5967CCE9FC841BA29D2C5D";
            cipherText = cipherText.Replace(" ", "+");
            var cipherBytes = Convert.FromBase64String(cipherText);
            using (var encryptor = Aes.Create())
            {
                var pdb = new Rfc2898DeriveBytes(encryptionKey, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                if (encryptor != null)
                {
                    encryptor.Key = pdb.GetBytes(32);
                    encryptor.IV = pdb.GetBytes(16);
                    using (var ms = new MemoryStream())
                    {
                        using (var cs = new CryptoStream(ms, encryptor.CreateDecryptor(), CryptoStreamMode.Write))
                        {
                            cs.Write(cipherBytes, 0, cipherBytes.Length);
                            cs.Close();
                        }
                        cipherText = Encoding.Unicode.GetString(ms.ToArray());
                    }
                }
            }
            return cipherText;
        }

        private static string EncryptClaim(string clearText)
        {
            const string encryptionKey = "FFF301A39F5967CCE9FC841BA29D2C5D";
            var clearBytes = Encoding.Unicode.GetBytes(clearText);
            using (var encryptor = Aes.Create())
            {
                var pdb = new Rfc2898DeriveBytes(encryptionKey, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                if (encryptor != null)
                {
                    encryptor.Key = pdb.GetBytes(32);
                    encryptor.IV = pdb.GetBytes(16);
                    using (var ms = new MemoryStream())
                    {
                        using (var cs = new CryptoStream(ms, encryptor.CreateEncryptor(), CryptoStreamMode.Write))
                        {
                            cs.Write(clearBytes, 0, clearBytes.Length);
                            cs.Close();
                        }
                        clearText = Convert.ToBase64String(ms.ToArray());
                    }
                }
            }
            return clearText;
        }

        #endregion
    }
}
