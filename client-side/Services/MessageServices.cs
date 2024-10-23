﻿
using client_side.Models;
using NetMQ;
using server_side.Cryptography;
using Newtonsoft.Json;
using client_side.Services.Interfaces;
using DotNetEnv;
using server_side.Services;


namespace client_side.Services
{
    public class MessagesServices : IMessagesServices
    {
        private readonly string _rsa_public_key;
        private FolderPathServices folderpath;

        
        public MessagesServices()
        {
            folderpath = new FolderPathServices();
            var envGenerator = new GenerateEnvFile();
            envGenerator.EnvFileGenerator();
            Env.Load(folderpath.GetWattWiseFolderPath() + "\\server-side\\.env");
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
            var encryptMessage = new HandleEncryption();
            var result = encryptMessage.ApplyEncryption(modelData, key, iv,_rsa_public_key );
            messageToServer.Append(Convert.ToBase64String(result.encryptedKey));
            messageToServer.Append(Convert.ToBase64String(result.encryptedIv));
            messageToServer.Append(result.hashJson); //4
            messageToServer.Append(result.base64EncryptedData); //5
            return messageToServer;
        }
    }
}