namespace ClsSecurity
{
    using System;
    using System.Security.Cryptography;
    using System.Text;

    public class clsSecurity
    {
        private const string cipher = "cacheit987987987";

        public static string TripDESCrypt(string input)
        {
            byte[] MessageByte = Encoding.Default.GetBytes(input);
            TripleDESCryptoServiceProvider desEncrypt = new TripleDESCryptoServiceProvider();
            desEncrypt.KeySize = 0x80;
            desEncrypt.Mode = CipherMode.ECB;
            byte[] TripDESKey = Encoding.Default.GetBytes("cacheit987987987");
            desEncrypt.Key = TripDESKey;
            return Convert.ToBase64String(desEncrypt.CreateEncryptor().TransformFinalBlock(MessageByte, 0, MessageByte.Length));
        }

        public static string TripDesDecrypt(string input)
        {
            TripleDESCryptoServiceProvider desDecrypt = new TripleDESCryptoServiceProvider();
            desDecrypt.KeySize = 0x80;
            desDecrypt.Mode = CipherMode.ECB;
            desDecrypt.Key = Encoding.Default.GetBytes("cacheit987987987");
            byte[] DataByte = Convert.FromBase64String(input);
            return Encoding.Default.GetString(desDecrypt.CreateDecryptor().TransformFinalBlock(DataByte, 0, DataByte.Length));
        }
    }
}

