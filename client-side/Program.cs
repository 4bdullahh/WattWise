using System.Reflection;
using NetMQ;
using NetMQ.Sockets;
using Newtonsoft.Json;
using server_side.Cryptography;
using System.Security.Cryptography;
using System.Text;
using client_side.Models;
using DotNetEnv;

namespace client_side
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            string serverSideFolderPath = GetClientSideFolderPath();
            Env.Load(serverSideFolderPath + "\\.env");
            string serverPublicKey = Env.GetString("RSA_PUBLIC_KEY");

            byte[] key = new byte[32];
            byte[] iv = new byte[16];

            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(key);
                rng.GetBytes(iv);
            }
            var clientSocketPerThread = new ThreadLocal<DealerSocket>();
            using (var poller = new NetMQPoller())
            {
                var maxClients = 20;
                var minInterval = TimeSpan.FromMilliseconds(3000);
                // var maxInterval = TimeSpan.FromMilliseconds(60000);
                Task.Factory.StartNew(state =>
                {
                    DealerSocket client = null;
                    if (!clientSocketPerThread.IsValueCreated)
                    {
                        client = new DealerSocket();
                        client.Connect("tcp://localhost:5555");
                        client.Options.Identity = Encoding.UTF8.GetBytes(state.ToString());
                        client.ReceiveReady += (s, e) =>
                        {
                            var message = e.Socket.ReceiveMultipartMessage();
                            var response = message[1].ConvertToString();
                            Console.WriteLine($"Server Recieved: {response}");
                        };
                        clientSocketPerThread.Value = client;
                        poller.Add(client);
                    }
                    else
                    {
                        client = clientSocketPerThread.Value;
                    }
                    var timer = new NetMQTimer(minInterval);
                    poller.Add(timer);
                    timer.Elapsed += (sender, e) =>
                    {
                        var messageToServer = new NetMQMessage();
                        messageToServer.Append(state.ToString()); //0
                        messageToServer.AppendEmptyFrame(); //1

                        var userData = new UserModel
                        {
                            UserID = 204,
                            UserEmail = "fedhf@hotmail",
                            Topic = "addUser"
                            // SmartMeterId = 202,
                            // EnergyPerKwH = 24.5,
                            // CurrentMonthCost = 20.5
                        };
                        // var currentInterval = new Random();

                        var jsonRequest = JsonConvert.SerializeObject(userData);
                        string hashJson = Cryptography.GenerateHash(jsonRequest);
                        byte[] encryptedData = Cryptography.AESEncrypt(jsonRequest, key, iv);
                        string base64EncryptedData = Convert.ToBase64String(encryptedData);
                        // We might use this later for Electron
                        //var topic = userData.Topic;
                        byte[] encryptedKey = Cryptography.RSAEncrypt(serverPublicKey, key);
                        byte[] encryptedIv = Cryptography.RSAEncrypt(serverPublicKey, iv);

                        messageToServer.Append(Convert.ToBase64String(encryptedKey));
                        messageToServer.Append(Convert.ToBase64String(encryptedIv));
                        messageToServer.Append(hashJson); //4
                        messageToServer.Append(base64EncryptedData); //5
                        Console.WriteLine("Waiting for message...");
                        client.SendMultipartMessage(messageToServer);
                    };
                }, TaskCreationOptions.LongRunning);
                poller.RunAsync();
                Console.Read();
                poller.Stop();
            }
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
