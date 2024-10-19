using System.Net.Security;
using System.Net.Sockets;
using NetMQ;
using NetMQ.Sockets;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using client_side.Models;
using client_side.Services.Interfaces;
using server_side.Services;

namespace client_side.Services
{
    public class ClientServices : IClientServices
    {

        private readonly IMessagesServices _messagesServices;
        private X509Certificate2 _clientCertificate;
        private FolderPathServices folderpath;

        public ClientServices(IMessagesServices messagesServices)
        {
            folderpath= new FolderPathServices();
            _messagesServices = messagesServices;
            _clientCertificate = new X509Certificate2(folderpath.GetClientFolderPath() + "\\client_certificate.pfx", "John@Muhammad@Vinny");
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
                        
                        // The first thing I want is to establish a secure SSL/TLS connection
                        // For the secure connection I am using port 5556
                        TcpClient tcpClient = new TcpClient("localhost", 5556); 

                        using (var sslStream = new SslStream(tcpClient.GetStream(), false, (sender, cert, chain, errors) => true))
                        {
                            sslStream.AuthenticateAsClient("localhost", new X509CertificateCollection { _clientCertificate }, false);
                            Console.WriteLine("Client: TLS complete!!");
                        }
                        
                        DealerSocket client = null;

                        if (!clientSocketPerThread.IsValueCreated)
                        {
                            client = new DealerSocket();
                            client.Options.Identity = Encoding.UTF8.GetBytes(state.ToString());
                            // Here is when I changed the route again
                            client.Connect("tcp://localhost:5555");
                            
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

