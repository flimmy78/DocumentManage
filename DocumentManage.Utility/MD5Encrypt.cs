using System;
using System.Text;
using System.Security.Cryptography;

namespace DocumentManage.Utility
{
    //DocumentManage.Utility 空间 -- Md5类
    public sealed class Md5Encrypt
    {
        /// <summary>
        /// 取得输入字符串的MD5哈希值
        /// </summary>
        /// <param name="argInput">输入字符串</param>
        /// <returns>MD5哈希值</returns>
        public static string GetMd5Hash(string argInput)
        {
            // Create a new instance of the MD5CryptoServiceProvider object.
            MD5 md5Hasher = MD5.Create();

            // Convert the input string to a byte array and compute the hash.
            byte[] data = md5Hasher.ComputeHash(Encoding.Default.GetBytes(argInput));

            // Create a new Stringbuilder to collect the bytes
            // and create a string.
            var sBuilder = new StringBuilder();

            // Loop through each byte of the hashed data 
            // and format each one as a hexadecimal string.
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }

            // Return the hexadecimal string.
            return sBuilder.ToString();
        }

        /// <summary>
        /// 验证MD5哈希值
        /// </summary>
        /// <param name="argInput">输入字符串</param>
        /// <param name="argHash">哈希值</param>
        /// <returns>相同返回TRUE,不同返回FALSE</returns>
        public static bool VerifyMd5Hash(string argInput, string argHash)
        {
            // Hash the input.
            string hashOfInput = GetMd5Hash(argInput);

            // Create a StringComparer an comare the hashes.
            StringComparer comparer = StringComparer.OrdinalIgnoreCase;

            if (0 == comparer.Compare(hashOfInput, argHash))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
