using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;


namespace Wes.Utilities
{
    public static class MD5HashProvider
    {
        public static string GetMD5HashFromFile(string fileName)
        {
            try
            {
                FileStream file = new FileStream(fileName, FileMode.Open);
                System.Security.Cryptography.MD5 md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
                byte[] retVal = md5.ComputeHash(file);
                file.Close();

                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < retVal.Length; i++)
                {
                    sb.Append(retVal[i].ToString("x2"));
                }
                return sb.ToString();
            }
            catch (System.Exception ex)
            {
                throw new System.Exception("GetMD5HashFromFile() fail,error:" + ex.Message);
            }
        }

        public static string GetBytesMd5String(this byte[] bytes)
        {
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] output = md5.ComputeHash(bytes);

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < output.Length; i++)
            {
                sb.Append(output[i].ToString("x2"));
            }
            return sb.ToString();
        }

        public static string GetStringMd5String(this string @this)
        {
            byte[] bytes = Encoding.Default.GetBytes(@this);
            return GetBytesMd5String(bytes);
        }
    }
}
