using NetMQ;
using NetMQ.Sockets;
using Newtonsoft.Json;
using server_side.Repository.Interface;
using server_side.Services.Interface;
using System.Text;
using DotNetEnv;
using server_side.Cryptography;

namespace server_side.Services
{
    public class MessageService : IMessageServices 
    {
        private readonly IUserServices _userServices;
        private readonly string _rsaPrivateKey;
        private readonly IFolderPathServices _folderPathServices;
        public MessageService(IFolderPathServices folderPathServices, IUserServices userServices)
        {
            _folderPathServices = folderPathServices;
            _userServices = userServices;
            string serverSideFolderPath = _folderPathServices.GetServerSideFolderPath();
            var envGenerator = new GenerateEnvFile();
            envGenerator.EnvFileGenerator();
            Env.Load(serverSideFolderPath + "\\.env");
            _rsaPrivateKey = Env.GetString("RSA_PRIVATE_KEY");
        }
        public void ReceiveMessageServices()
        {
            using (var server = new RouterSocket("@tcp://*:5555"))
            using (var poller = new NetMQPoller())
            {
                server.ReceiveReady += (s, e) =>
                {
                    var recievedMessage = server.ReceiveMultipartMessage();
                    Console.WriteLine($"Received message: {recievedMessage}");
                    var clientAddress = recievedMessage[0];
                    
                    byte[] encryptedKey = Convert.FromBase64String(Encoding.UTF8.GetString(recievedMessage[3].ToByteArray()));
                    byte[] encryptedIv = Convert.FromBase64String(Encoding.UTF8.GetString(recievedMessage[4].ToByteArray()));
                    byte[] key = Cryptography.Cryptography.RSADecrypt(_rsaPrivateKey, encryptedKey);
                    byte[] iv = Cryptography.Cryptography.RSADecrypt(_rsaPrivateKey, encryptedIv);
                    string receivedHash = Encoding.UTF8.GetString(recievedMessage[5].Buffer);
                    string receivedUser = Encoding.UTF8.GetString(recievedMessage[6].Buffer);
                    byte[] encryptedMessage = Convert.FromBase64String(receivedUser);
                    string decryptedMessage = Cryptography.Cryptography.AESDecrypt(encryptedMessage, key, iv);
                    string userHash = Cryptography.Cryptography.GenerateHash(decryptedMessage);

                    //This is for test when the data is temperaded
                    /*
                    UserData tempered = JsonConvert.DeserializeObject<UserData>(decryptedMessage);
                    tempered.UserID = 1000;
                    var temperedJson = JsonConvert.SerializeObject(tempered);
                    string userHash = Cryptography.Cryptography.GenerateHash(temperedJson);*/

                    if (userHash != receivedHash)
                    {
                        Console.WriteLine("Hash doesn't match");
                    }
                    else
                    {
                        var messageToClient = new NetMQMessage();
                        messageToClient.Append(clientAddress);
                        messageToClient.AppendEmptyFrame();
                        
                       var response = _userServices.UserOperations(decryptedMessage);
                       
                        var jsonResponse = JsonConvert.SerializeObject(response);
                        messageToClient.Append(jsonResponse);
                        server.SendMultipartMessage(messageToClient);
                        Console.WriteLine($"Sending to Client: {messageToClient}");
                    }
                };
                poller.Add(server);
                poller.RunAsync();
                Console.Read();
                poller.Stop();
            }
        }
    }
}