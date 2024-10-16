
using client_side.Models;
using NetMQ;
using server_side.Cryptography;
using Newtonsoft.Json;
using client_side.Services.Interfaces;


namespace client_side.Services
{
    public class MessagesServices : IMessagesServices
    {
        public MessagesServices()
        {
        }

        public NetMQMessage SendReading
        (
            string clientAddress,
            UserModel userData,
            byte[] key,
            byte[] iv
        )
        {
            var messageToServer = new NetMQMessage();
            messageToServer.Append(clientAddress); //0
            messageToServer.AppendEmptyFrame(); //1

            var jsonRequest = JsonConvert.SerializeObject(userData);
            string hashJson = Cryptography.GenerateHash(jsonRequest);
            byte[] encryptedData = Cryptography.Encrypt(jsonRequest, key, iv);
            string base64EncryptedData = Convert.ToBase64String(encryptedData);
            // We might use this later for Electron
            //var topic = userData.Topic;
            string base64Key = Convert.ToBase64String(key);
            string base64Iv = Convert.ToBase64String(iv);

            messageToServer.Append(base64Key); //2
            messageToServer.Append(base64Iv); //3
            messageToServer.Append(hashJson); //4
            messageToServer.Append(base64EncryptedData); //5
            return messageToServer;
        }
    }
}