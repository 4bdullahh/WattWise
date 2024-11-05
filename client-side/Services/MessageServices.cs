﻿
using client_side.Models;
using NetMQ;
using server_side.Cryptography;
using Newtonsoft.Json;
using client_side.Services.Interfaces;
using DotNetEnv;
using server_side.Repository.Interface;
using server_side.Repository.Models;
using server_side.Services;
using server_side.Services.Interface;


namespace client_side.Services
{
    public class MessagesServices : IMessagesServices
    {
        
        private readonly string _rsa_public_key;
        private readonly IFolderPathServices folderPath;
        private readonly IErrorLogRepo _errorLogRepo;
        private readonly ErrorLogMessage _errorLogMessage;
        
        
        public MessagesServices(IFolderPathServices folderPath, IErrorLogRepo errorLogRepo)
        {
            this.folderPath= folderPath;
            _errorLogRepo = errorLogRepo;
            _errorLogMessage = new ErrorLogMessage();
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
                var _message = new NetMQMessage();
                _message.Append(clientAddress); //0
                _message.AppendEmptyFrame(); //1
                var encryptMessage = new HandleEncryption();
                var result = encryptMessage.ApplyEncryptionClient(modelData, key, iv, _rsa_public_key);
                _message.Append(Convert.ToBase64String(result.encryptedKey));
                _message.Append(Convert.ToBase64String(result.encryptedIv));
                _message.Append(result.hashJson); //4
                _message.Append(result.base64EncryptedData); //5
                return _message;
            }
            catch (Exception e)
            {
                _errorLogMessage.Message = $"Client: ClientID {_errorLogMessage.ClientId} Message did not sent to server, error: : {e.Message} : {DateTime.UtcNow}";
                Console.WriteLine($"{_errorLogMessage.Message} {e.Message}");
                _errorLogRepo.LogError(_errorLogMessage);
                throw;
            }
        }
    }
}