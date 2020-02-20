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
        /// <param name="source">待解密的字符串</param>
        /// <returns></returns>
        public static string Get32MD5(string source)
        {
            using (MD5 md5Hash = MD5.Create())
            {
                byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(source));
                StringBuilder sBuilder = new StringBuilder();
                for (int i = 0; i < data.Length; i++)
                {
                    sBuilder.Append(data[i].ToString("X2"));
                }

                string hash = sBuilder.ToString();
                return hash.ToUpper();
            }
        }

        /// <summary>
        /// 获取16位md5加密
        /// </summary>
        /// <param name="source">待解密的字符串</param>
        /// <returns></returns>
        public static string Get16MD5(string source)
        {
            using (MD5 md5Hash = MD5.Create())
            {
                byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(source));
                //转换成字符串，并取9到25位
                string sBuilder = BitConverter.ToString(data, 4, 8);
                //BitConverter转换出来的字符串会在每个字符中间产生一个分隔符，需要去除掉
                sBuilder = sBuilder.Replace("-", "");
                return sBuilder.ToString().ToUpper();
            }
        }
    }
}
