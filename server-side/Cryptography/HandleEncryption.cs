﻿using System.Security.Cryptography;
using System.Text;
using NetMQ;
using Newtonsoft.Json;
using server_side.Services.Models;

namespace server_side.Cryptography;

public class HandleEncryption
{
    /*
     * Class Documentation:
        This class is responsible for handle encryption and include methods for:
            Apply Encryption and Decryption of messages from server and client
            Generate Key and Iv which helps to transmit secure messages between client and server
     */
    
    public (string userHash, string decryptedMessage, string receivedHash) 
        ApplyDencryption(NetMQMessage receivedMessage, byte[] encryptedKey, byte[] encryptedIv, string receivedHash, string receivedUser, string _rsaPrivateKey)
    {
        try
        {
            encryptedKey = Convert.FromBase64String(Encoding.UTF8.GetString(receivedMessage[3].ToByteArray()));
            encryptedIv = Convert.FromBase64String(Encoding.UTF8.GetString(receivedMessage[4].ToByteArray()));
            byte[] key = Cryptography.RSADecrypt(_rsaPrivateKey, encryptedKey);
            byte[] iv = Cryptography.RSADecrypt(_rsaPrivateKey, encryptedIv);
            receivedHash = Encoding.UTF8.GetString(receivedMessage[5].Buffer);
            receivedUser = Encoding.UTF8.GetString(receivedMessage[6].Buffer);
            byte[] encryptedMessage = Convert.FromBase64String(receivedUser);
            string decryptedMessage = Cryptography.AESDecrypt(encryptedMessage, key, iv);
            string userHash = Cryptography.GenerateHash(decryptedMessage);
            return (userHash, decryptedMessage, receivedHash);
        }
        catch (Exception e)
        {
            Console.WriteLine($"We could not decrypt message: {e.Message}");
            throw;
        }
    }

    public (string hashJson, byte[] encryptedKey, byte[] encryptedIv, string base64EncryptedData )
        ApplyEncryptionClient<T>(T jsonData, byte[] key, byte[] iv, string _rsa_public_key)
    {
        try
        {
            var jsonRequest = JsonConvert.SerializeObject(jsonData);
            string hashJson = Cryptography.GenerateHash(jsonRequest);
            byte[] encryptedData = Cryptography.AESEncrypt(jsonRequest, key, iv);
            string base64EncryptedData = Convert.ToBase64String(encryptedData);
            byte[] encryptedKey = Cryptography.RSAEncrypt(_rsa_public_key, key);
            byte[] encryptedIv = Cryptography.RSAEncrypt(_rsa_public_key, iv);
            return (hashJson, encryptedKey, encryptedIv, base64EncryptedData);
        }
        catch (Exception e)
        {
            Console.WriteLine($"We could not encrypt message: {e.Message}");
            throw;
        }
    }
    public (string hashJson, byte[] encryptedKey, byte[] encryptedIv, string base64EncryptedData )
        ApplyEncryptionServer<T>(T jsonData, byte[] key, byte[] iv, string _rsa_public_key)
    {
        try
        {
            var jsonRequest = JsonConvert.SerializeObject(jsonData);
            string hashJson = Cryptography.GenerateHash(jsonRequest);
            byte[] encryptedData = Cryptography.AESEncrypt(jsonRequest, key, iv);
            string base64EncryptedData = Convert.ToBase64String(encryptedData);
            byte[] encryptedKey = Cryptography.RSAEncrypt(_rsa_public_key, key);
            byte[] encryptedIv = Cryptography.RSAEncrypt(_rsa_public_key, iv);
            return (hashJson, encryptedKey, encryptedIv, base64EncryptedData);
        }
        catch (Exception e)
        {
            Console.WriteLine($"We could not encrypt message: {e.Message}");
            throw;
        }
    }
    public (string userHash, string decryptedMessage, string receivedHash) 
        ApplyDencryptionServer(NetMQMessage recievedMessage, byte[] encryptedKey, byte[] encryptedIv, string receivedHash, string receivedUser, string _rsaPrivateKey)
    {
        try
        {
            encryptedKey = Convert.FromBase64String(Encoding.UTF8.GetString(recievedMessage[1].ToByteArray()));
            encryptedIv = Convert.FromBase64String(Encoding.UTF8.GetString(recievedMessage[2].ToByteArray()));
            byte[] key = Cryptography.RSADecrypt(_rsaPrivateKey, encryptedKey);
            byte[] iv = Cryptography.RSADecrypt(_rsaPrivateKey, encryptedIv);
            receivedHash = Encoding.UTF8.GetString(recievedMessage[3].Buffer);
            receivedUser = Encoding.UTF8.GetString(recievedMessage[4].Buffer);
            byte[] encryptedMessage = Convert.FromBase64String(receivedUser);
            string decryptedMessage = Cryptography.AESDecrypt(encryptedMessage, key, iv);
            string userHash = Cryptography.GenerateHash(decryptedMessage);

            return (userHash, decryptedMessage, receivedHash);
        }
        catch (Exception e)
        {
            Console.WriteLine($"We could not decrypt message: {e.Message}");
            throw;
        }
    }
    public (byte[] key, byte[] iv) 
        GenerateKeys()
    {
        try
        {
            byte[] key = new byte[32];
            byte[] iv = new byte[16];
            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(key);
                rng.GetBytes(iv);
            }

            return (key, iv);
        }
        catch (Exception e)
        {
            Console.WriteLine($"We could not generate the keys, error: {e.Message}");
            throw;
        }
    }
    
}