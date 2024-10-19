
using client_side.Models;
using NetMQ;
using server_side.Cryptography;
using Newtonsoft.Json;
using client_side.Services.Interfaces;
using DotNetEnv;


namespace client_side.Services
{
    public class MessagesServices : IMessagesServices
    {
        private readonly string _rsa_public_key;
        
        public MessagesServices()
        {
            string serverSideFolderPath = GetClientSideFolderPath();
            Env.Load(serverSideFolderPath + "\\.env");
            _rsa_public_key = Env.GetString("RSA_PUBLIC_KEY");

        }

        
        public NetMQMessage SendReading<T>
        (
            string clientAddress,
            T modelData,
            byte[] key,
            byte[] iv
        )
        {
            
            var messageToServer = new NetMQMessage();
            messageToServer.Append(clientAddress); //0
            messageToServer.AppendEmptyFrame(); //1

            var jsonRequest = JsonConvert.SerializeObject(modelData);
            string hashJson = Cryptography.GenerateHash(jsonRequest);
            byte[] encryptedData = Cryptography.AESEncrypt(jsonRequest, key, iv);
            string base64EncryptedData = Convert.ToBase64String(encryptedData);
            // We might use this later for Electron
            //var topic = userData.Topic;
            byte[] encryptedKey = Cryptography.RSAEncrypt(_rsa_public_key, key);
            byte[] encryptedIv = Cryptography.RSAEncrypt(_rsa_public_key, iv);

            messageToServer.Append(Convert.ToBase64String(encryptedKey));
            messageToServer.Append(Convert.ToBase64String(encryptedIv));
            messageToServer.Append(hashJson); //4
            messageToServer.Append(base64EncryptedData); //5
            return messageToServer;
        }
        
        private static string GetClientSideFolderPath()
        {
            string folderName = "client-side";
            var currentDirectory = new DirectoryInfo(Environment.CurrentDirectory);
            while (currentDirectory != null && currentDirectory.Name != folderName)
            {
                currentDirectory = currentDirectory.Parent;
            }
            if (currentDirectory == null)
            {
                throw new DirectoryNotFoundException($"Could not find the '{folderName}' directory.");
            }
            return currentDirectory.FullName;
        }
    }
}