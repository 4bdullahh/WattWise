using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using NetMQ;
using NetMQ.Sockets;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using client_side.Models;
using client_side.Services.Interfaces;
using server_side.Cryptography;
using server_side.Services;
using System.IO.Pipes;

namespace client_side.Services
{
    public class ClientServices : IClientServices
    {

        private readonly IMessagesServices _messagesServices;
        private X509Certificate2 _clientCertificate;
        private FolderPathServices folderpath;

        public ClientServices(IMessagesServices messagesServices)
        {
            folderpath = new FolderPathServices();
            _messagesServices = messagesServices;
            _clientCertificate = new X509Certificate2(folderpath.GetClientFolderPath() + "\\client_certificate.pfx", "a2bf39b00064f4163c868d075b35a2a28b87cf0f471021f7578f866851dc866f");
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

                       TcpClient tcpClient = new TcpClient("localhost", 5556);

                       using (var sslStream = new SslStream(tcpClient.GetStream(), false, (sender, cert, chain, errors) => true))
                       {
                           sslStream.AuthenticateAsClient("localhost", new X509CertificateCollection { _clientCertificate }, SslProtocols.Tls12, false);
                           if (sslStream.IsAuthenticated)
                           {
                               Console.WriteLine("Client: TLS authentication successful!");
                           }
                           else
                           {
                               Console.WriteLine("Client: TLS authentication failed!");
                           }
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

                           // var modelData = new SmartDevice
                           // {
                           //     SmartMeterID = 205, 
                           //     EnergyPerKwH = 20.5, 
                           //     CurrentMonthCost = 200
                           // };

                           var modelData = new UserData
                           {
                               UserID = 607,
                               UserEmail = "addyfive@gmail.com",
                               Topic = "addUser"
                           };

                           var messageToServer = _messagesServices.SendReading(
                                   clientAddress,
                                   modelData,
                                   key,
                                   iv
                               );

                           Console.WriteLine($"ClientId: {clientId}, Waiting for message...");
                           client.SendMultipartMessage(messageToServer);

                           int newTime = currentInterval.Next(minInterval, maxInterval);
                           timer.Interval = newTime;
                           Console.WriteLine($"New time {newTime}");
                       };

                   }, i, TaskCreationOptions.LongRunning);
                }

                poller.RunAsync();
                Console.Read();
                poller.Stop();
            }

        }

        public async Task ElectronServerAsync()
        {
            Task getByIdPipeTask = Task.Factory.StartNew(async () =>
            {
                while (true)
                {
                    using (var server = new NamedPipeServerStream("get-by-id"))
                    {
                        server.WaitForConnection();
                        using (StreamReader reader = new StreamReader(server))
                        using (StreamWriter writer = new StreamWriter(server))
                        {
                            char[] buffer = new char[1024];
                            int numRead;

                            while ((numRead = await reader.ReadAsync(buffer, 0, buffer.Length)) > 0)
                            {
                                string receivedMessage = new string(buffer, 0, numRead);
                                if (receivedMessage == "getData")
                                {
                                    // CODE TO GET BY ID
                                }
                                Console.WriteLine($"Received: {receivedMessage}");

                                // RETURN THE DATA 
                                await writer.WriteLineAsync("GET BY ID DATA");
                                await writer.FlushAsync();
                            }
                        }
                    }

                }
            }, TaskCreationOptions.LongRunning);

            Task basePipeTask = Task.Factory.StartNew(async () =>
            {
                while (true)
                {
                    using (var server = new NamedPipeServerStream("base-pipe"))
                    {
                        server.WaitForConnection();
                        using (StreamReader reader = new StreamReader(server))
                        using (StreamWriter writer = new StreamWriter(server))
                        {
                            char[] buffer = new char[1024];
                            int numRead;

                            while ((numRead = await reader.ReadAsync(buffer, 0, buffer.Length)) > 0)
                            {
                                string receivedMessage = new string(buffer, 0, numRead);
                                Console.WriteLine($"Received: {receivedMessage}");

                                await writer.WriteLineAsync("Message received: " + receivedMessage);
                                await writer.FlushAsync();
                            }
                        }
                    }

                }
            }, TaskCreationOptions.LongRunning);

            await Task.WhenAll(getByIdPipeTask, basePipeTask);
        }

    }

}

