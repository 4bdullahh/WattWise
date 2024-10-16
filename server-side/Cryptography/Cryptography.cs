using System.Security.Cryptography;
using System.Text;

namespace server_side.Cryptography;

public static class Cryptography
{
    public static string GenerateHash(this string value)
    {
        var hash = SHA256.Create();
        var encode = new ASCIIEncoding();
        var array = encode.GetBytes(value);
        array = hash.ComputeHash(array);

        var strHexa = new StringBuilder();

        foreach (var item in array)
        {
            strHexa.Append(item.ToString("x2"));
        }

        return strHexa.ToString();
    }
    public static (string publicKey, string privateKey) GenerateRSAKeys()
    {
        using (var rsa = new RSACryptoServiceProvider(2048))
        {
            rsa.PersistKeyInCsp = false;
            var publicKey = Convert.ToBase64String(rsa.ExportRSAPublicKey());
            var privateKey = Convert.ToBase64String(rsa.ExportRSAPrivateKey());
            return (publicKey, privateKey);
        }
    }
    public static byte[] RSAEncrypt(string publicKey, byte[] data)
    {
        using (var rsa = new RSACryptoServiceProvider(2048))
        {
            rsa.PersistKeyInCsp = false;

            rsa.FromXmlString(publicKey);

            return rsa.Encrypt(data, RSAEncryptionPadding.Pkcs1);
        }
    }
    public static byte[] RSADecrypt(string privateKey, byte[] data)
    {
        using (var rsa = new RSACryptoServiceProvider(2048))
        {
            rsa.PersistKeyInCsp = false;
            rsa.FromXmlString(privateKey);
            
            return rsa.Decrypt(data, RSAEncryptionPadding.Pkcs1);
        }
    }
    public static byte[] AESEncrypt(string plainText, byte[] key, byte[] iv)
    {
        using (Aes aes = Aes.Create())
        {
            aes.Key = key;
            aes.IV = iv;

            using (var encryptor = aes.CreateEncryptor(aes.Key, aes.IV))
            using (var ms = new MemoryStream())
            {
                using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                using (var writer = new StreamWriter(cs))
                {
                    writer.Write(plainText);
                }
                return ms.ToArray();
            }
        }
    }
    public static string AESDecrypt(byte[] cipherText, byte[] key, byte[] iv)
    {
        using (Aes aes = Aes.Create())
        {
            aes.Key = key;
            aes.IV = iv;

            using (var decryptor = aes.CreateDecryptor(aes.Key, aes.IV))
            using (var ms = new MemoryStream(cipherText))
            using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
            using (var reader = new StreamReader(cs))
            {
                return reader.ReadToEnd();
            }
        }
    }
}
