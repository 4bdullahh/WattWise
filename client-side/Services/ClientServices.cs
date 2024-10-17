using NetMQ;
using NetMQ.Sockets;
using System.Security.Cryptography;
using System.Text;
using client_side.Models;
using client_side.Services.Interfaces;

namespace client_side.Services
{
    public class ClientServices : IClientServices
    {

        private readonly IMessagesServices _messagesServices;

        public ClientServices(IMessagesServices messagesServices)
        {
            _messagesServices = messagesServices;
        }

        public void StartClient()
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
                var maxClients = 1;
                int firstSend = 300;
                int minInterval = 1000;
                int maxInterval = 3000;
                var currentInterval = new Random();

                for (int i = 0; i < maxClients; i++)
                {
                    
                    int clientId = i;
                    
                     Task.Factory.StartNew(state =>
                    {
                        Console.WriteLine($"Client: {clientId} started");
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
                                Console.WriteLine($"Server Recieved: {response}, ClientId: {clientId}");
                            };
                            clientSocketPerThread.Value = client;
                            poller.Add(client);
                        }
                        else
                        {
                            client = clientSocketPerThread.Value;
                        }
                        
                        var timer = new NetMQTimer(firstSend);
                        poller.Add(timer);
                        
                        
                        
                        timer.Elapsed += (sender, e) =>
                        {
                            string clientAddress = state.ToString();
                            
                            var userData = new UserModel
                            {
                                UserID = 204, 
                                UserEmail = "manchester@hotmail", 
                                Topic = "UpdateUser"
                            };
                            
                             var messageToServer = _messagesServices.SendReading(
                                    clientAddress,
                                    userData,
                                    key,
                                    iv
                                );
                            
                            Console.WriteLine($"ClientId: {clientId}, Waiting for message...");
                            client.SendMultipartMessage(messageToServer);
                            
                            int newTime = currentInterval.Next(minInterval, maxInterval);
                            timer.Interval = newTime;
                            Console.WriteLine($"New time {newTime}");
                        };
                      
                    },i , TaskCreationOptions.LongRunning); 
                }
                
                poller.RunAsync();
                Console.Read();
                poller.Stop();
            }
            
        }
        
    
        
    }
    
}

