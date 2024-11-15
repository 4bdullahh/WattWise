using System.Security.Cryptography;
using System.Text;

namespace server_side.Cryptography;

public static class Cryptography
{
    
    /*
     * Class Documentation:
        This class is responsible for cryptography includes methods for:
            Generating Hash
            Public and Private Key Generator
            AES/RSA Encryption and Decryption Handle
     */
    public static string GenerateHash(this string value)
    {
        try
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
        catch (Exception e)
        {
            Console.WriteLine($"We could not generate hash: {e.Message}");
            throw;
        }
    }
    public static (string publicKey, string privateKey) GenerateRsaKeys()
    {
        try
        {
            using (var rsa = new RSACryptoServiceProvider(2048))
            {
                rsa.PersistKeyInCsp = false;
                string publicKey = rsa.ToXmlString(false); 
                string privateKey = rsa.ToXmlString(true); 
                return (publicKey, privateKey);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"We could not generate RSA keys: {e.Message}");
            throw;
        }
    }
    public static byte[] RSAEncrypt(string publicKey, byte[] data)
    {
        try
        {
            using (var rsa = new RSACryptoServiceProvider(2048))
            {
                rsa.PersistKeyInCsp = false;

                rsa.FromXmlString(publicKey);

                return rsa.Encrypt(data, RSAEncryptionPadding.Pkcs1);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"We could not RSA Encrypt: {e.Message}");
            throw;
        }
    }
    public static byte[] RSADecrypt(string privateKey, byte[] data)
    {
        try
        {
            using (var rsa = new RSACryptoServiceProvider(2048))
            {
                rsa.PersistKeyInCsp = false;
                rsa.FromXmlString(privateKey);

                return rsa.Decrypt(data, RSAEncryptionPadding.Pkcs1);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"We could not RSA Decrypt: {e.Message}");
            throw;
        }
    }
    public static byte[] AESEncrypt(string plainText, byte[] key, byte[] iv)
    {
        try
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
        catch (Exception e)
        {
            Console.WriteLine($"We could not AES Encrypt: {e.Message}");
            throw;
        }
    }
    public static string AESDecrypt(byte[] cipherText, byte[] key, byte[] iv)
    {
        try
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
        catch (Exception e)
        {
            Console.WriteLine($"We could not AES Decrypt: {e.Message}");
            throw;
        }
    }
}
