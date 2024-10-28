﻿
using client_side.Models;
using NetMQ;
using server_side.Cryptography;
using Newtonsoft.Json;
using client_side.Services.Interfaces;
using DotNetEnv;
using server_side.Services;
using server_side.Services.Interface;


namespace client_side.Services
{
    public class MessagesServices : IMessagesServices
    {
        private readonly string _rsa_public_key;
        private readonly IFolderPathServices folderPath;
        
        public MessagesServices(IFolderPathServices folderPath)
        {
            this.folderPath= folderPath;
            var envGenerator = new GenerateEnvFile(folderPath);
            envGenerator.EnvFileGenerator();
            Env.Load(folderPath.GetWattWiseFolderPath() + "\\server-side\\.env");
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
            try
            {
                var messageToServer = new NetMQMessage();
                messageToServer.Append(clientAddress); //0
                messageToServer.AppendEmptyFrame(); //1
                var encryptMessage = new HandleEncryption();
                var result = encryptMessage.ApplyEncryptionClient(modelData, key, iv, _rsa_public_key);
                messageToServer.Append(Convert.ToBase64String(result.encryptedKey));
                messageToServer.Append(Convert.ToBase64String(result.encryptedIv));
                messageToServer.Append(result.hashJson); //4
                messageToServer.Append(result.base64EncryptedData); //5
                return messageToServer;
            }
            catch (Exception e)
            {
                Console.WriteLine($"We could not send the message to the server, error: {e.Message}");
                throw;
            }
        }
    }
}