using System.Reflection;
using NetMQ;
using NetMQ.Sockets;
using Newtonsoft.Json;
using server_side.Cryptography;
using System.Security.Cryptography;
using System.Text;
using client_side.Models;

namespace client_side
{
    internal class Program
    {
        private static void Main(string[] args)
        {
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
                int minInterval = 5000;
                int maxInterval = 20000;
                var currentInterval = new Random();

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
                        int sendTime = currentInterval.Next(minInterval, maxInterval);
                        var timer = new NetMQTimer(sendTime);
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
                                Topic = ""
                            };
                            
                                
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
                            messageToServer.Append(hashJson);  //4
                            messageToServer.Append(base64EncryptedData); //5
                            Console.WriteLine("Waiting for message...");
                            Console.WriteLine($"Old time {sendTime}");
                            client.SendMultipartMessage(messageToServer);
                            
                            timer.EnableAndReset();
                            int newTime = currentInterval.Next(minInterval, maxInterval);
                            timer.Interval = newTime;
                            Console.WriteLine($"New time {newTime}");
                        };
                      
                    }, TaskCreationOptions.LongRunning); 
                
                
                poller.RunAsync();
                Console.Read();
                poller.Stop();
            }
        }

 
    }
}