using System;
using System.Security.Cryptography;
using System.Text;

namespace Demo.Business
{
    public class DecryptBusiness
    {
        public static string Decrypt(
            string key,
            string cipherText,
            CipherMode cipherMode = CipherMode.CBC,
            PaddingMode paddingMode = PaddingMode.PKCS7,
            Encoding encoding = null)
        {
            if (encoding == null)
            {
                encoding = Encoding.UTF8;
            }

            using (SymmetricAlgorithm provider = new AesCryptoServiceProvider())
            {
                provider.Key = FromHex(key);
                provider.Mode = cipherMode;
                provider.Padding = paddingMode;
                provider.IV = FromHex("00000000000000000000000000000000");

                byte[] bytes = FromHex(cipherText);

                return encoding.GetString(provider
                    .CreateDecryptor()
                    .TransformFinalBlock(bytes, 0, bytes.Length));
            }
        }

        public static byte[] FromHex(string hex)
        {
            hex = hex.Replace("-", "");
            byte[] raw = new byte[hex.Length / 2];
            for (int i = 0; i < raw.Length; i++)
            {
                raw[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
            }
            return raw;
        }
    }
}
