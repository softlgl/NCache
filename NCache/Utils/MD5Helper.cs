using System;
using System.Security.Cryptography;
using System.Text;

namespace NCache.Utils
{
    internal class MD5Helper
    {
        /// <summary>
        ///获取32位md5加密
        /// </summary>
        /// <param name="source">待加密的字符串</param>
        /// <returns></returns>
        public static string Get32MD5(string source)
        {
            using (MD5 md5Hash = MD5.Create())
            {
                byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(source));
                StringBuilder sBuilder = new StringBuilder(data.Length * 2);
                for (int i = 0; i < data.Length; i++)
                {
                    // X2 本身就是大写，无需再 ToUpper
                    sBuilder.Append(data[i].ToString("X2"));
                }

                return sBuilder.ToString();
            }
        }
    }
}
