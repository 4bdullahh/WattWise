using System.Text;
using NetMQ;
using server_side.Services.Models;

namespace server_side.Cryptography;

public class HandleEncryption
{
    public (string userHash, string decryptedMessage, string receivedHash) 
        ApplyEncryption(NetMQMessage recievedMessage, byte[] encryptedKey, byte[] encryptedIv, string receivedHash, string receivedUser, string _rsaPrivateKey)
    {
        encryptedKey = Convert.FromBase64String(Encoding.UTF8.GetString(recievedMessage[3].ToByteArray()));
        encryptedIv = Convert.FromBase64String(Encoding.UTF8.GetString(recievedMessage[4].ToByteArray()));
        byte[] key = Cryptography.RSADecrypt(_rsaPrivateKey, encryptedKey);
        byte[] iv = Cryptography.RSADecrypt(_rsaPrivateKey, encryptedIv);
        receivedHash = Encoding.UTF8.GetString(recievedMessage[5].Buffer);
        receivedUser = Encoding.UTF8.GetString(recievedMessage[6].Buffer);
        byte[] encryptedMessage = Convert.FromBase64String(receivedUser);
        string decryptedMessage = Cryptography.AESDecrypt(encryptedMessage, key, iv);
        string userHash = Cryptography.GenerateHash(decryptedMessage);
            
        return (userHash, decryptedMessage, receivedHash);
    }
}