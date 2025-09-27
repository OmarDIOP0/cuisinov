//using DPWorldFramework.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace DPWorldEncryption
{
    public class SecurityManager
    {

        /// <summary>
        /// DecodeBase64
        /// </summary>
        /// <param name="base64Encoded"></param>
        /// <returns></returns>
        public static string DecodeBase64(string base64Encoded )
        {
            byte[] data = System.Convert.FromBase64String(base64Encoded);
            return Encoding.ASCII.GetString(data);
        }

        /// <summary>
        /// EncodeBase64
        /// </summary>
        /// <param name="base64Decoded"></param>
        /// <returns></returns>
        public static string EncodeBase64(string base64Decoded)
        {             
            byte[] data = System.Text.ASCIIEncoding.ASCII.GetBytes(base64Decoded);
            return System.Convert.ToBase64String(data);
        }

        /// <summary>
        /// SHA512
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string SHA512(string value)
        {
            SHA512Managed objHash = new SHA512Managed();
            return Hash(objHash, value);
        }

        /// <summary>
        /// Hash
        /// </summary>
        /// <param name="objHash"></param>
        /// <param name="value"></param>
        /// <param name="useHexa"></param>
        /// <returns></returns>
        private static string Hash(HashAlgorithm objHash, string value, bool useHexa = false)
        {
            string strReturn = string.Empty;
            StringBuilder objStringBuilder = new StringBuilder();
            byte[] arrByte;
            byte[] arrHash;
            byte[] arrHexa;
            int intLength;
            int i;

            if (value.Length > 0)
            {
                arrByte = SecurityConfig.Encoding.GetBytes(value);
                arrHash = objHash.ComputeHash(arrByte);

                if (!useHexa)
                    strReturn = Convert.ToBase64String(arrHash);
                else
                {
                    
                    arrHexa = SecurityConfig.Encoding.GetBytes(Convert.ToBase64String(arrHash));
                    intLength = arrHexa.Length - 1;

                    for (i = 0; i <= intLength; i++)
                        objStringBuilder.Append(arrHexa[i].ToString("x"));

                    strReturn = objStringBuilder.ToString();
                }
            }

            return strReturn;
        }


        /// <summary>
        /// EncryptAES
        /// </summary>
        /// <param name="strData"></param>
        /// <returns></returns>
        public static string EncryptAES(string strData)
        {

            return Convert.ToBase64String(Encrypt(Encoding.UTF8.GetBytes(strData)));
            // reference https://msdn.microsoft.com/en-us/library/ds4kkd55(v=vs.110).aspx

        }


        /// <summary>
        /// DecryptAES
        /// </summary>
        /// <param name="strData"></param>
        /// <returns></returns>
        public static string DecryptAES(string strData)
        {
            return Encoding.UTF8.GetString(Decrypt(Convert.FromBase64String(strData)));
            // reference https://msdn.microsoft.com/en-us/library/system.convert.frombase64string(v=vs.110).aspx

        }



        /// <summary>
        /// Encrypt
        /// </summary>
        /// <param name="strData"></param>
        /// <returns></returns>
        private static byte[] Encrypt(byte[] strData)
        {
            PasswordDeriveBytes passbytes =
                new PasswordDeriveBytes(Global.strPermutation,
                    new byte[] { Global.bytePermutation1,
                                 Global.bytePermutation2,
                                 Global.bytePermutation3,
                                 Global.bytePermutation4
            });

            MemoryStream memstream = new MemoryStream();
            Aes aes = new AesManaged();
            aes.Key = passbytes.GetBytes(aes.KeySize / 8);
            aes.IV = passbytes.GetBytes(aes.BlockSize / 8);

            CryptoStream cryptostream = new CryptoStream(memstream, aes.CreateEncryptor(), CryptoStreamMode.Write);
            cryptostream.Write(strData, 0, strData.Length);
            cryptostream.Close();

            return memstream.ToArray();
        }

        /// <summary>
        /// Decrypt
        /// </summary>
        /// <param name="strData"></param>
        /// <returns></returns>
        private static byte[] Decrypt(byte[] strData)
        {
            PasswordDeriveBytes passbytes =
                        new PasswordDeriveBytes(Global.strPermutation,
                                                new byte[] 
                                                { 
                                                    Global.bytePermutation1,
                                                    Global.bytePermutation2,
                                                    Global.bytePermutation3,
                                                    Global.bytePermutation4
                                                }
                                       );

            MemoryStream memstream = new MemoryStream();

            Aes aes = new AesManaged();

            aes.Key = passbytes.GetBytes(aes.KeySize / 8);
            aes.IV = passbytes.GetBytes(aes.BlockSize / 8);

            CryptoStream cryptostream = new CryptoStream(memstream, aes.CreateDecryptor(), CryptoStreamMode.Write);

            cryptostream.Write(strData, 0, strData.Length);
            cryptostream.Close();

            return memstream.ToArray();
        } 
    }
}