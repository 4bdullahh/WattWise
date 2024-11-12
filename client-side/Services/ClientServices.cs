using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using NetMQ;
using NetMQ.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using client_side.Models;
using client_side.Services.Interfaces;
using server_side.Cryptography;
using System.IO.Pipes;
using DotNetEnv;
using Microsoft.Extensions.Logging;
using server_side.Repository.Interface;
using server_side.Services.Interface;

namespace client_side.Services
{
    public class ClientServices : IClientServices
    {

        private readonly IMessagesServices _messagesServices;
        private X509Certificate2 _clientCertificate;
        private readonly string _rsaPrivateKey;
        private ICalculateCostClient _calculateCostClient;
        private readonly IFolderPathServices folderPath;
        private readonly ISmartMeterRepo _smartMeterRepo;
        public ClientServices(IMessagesServices messagesServices, ICalculateCostClient calculateCostClient, IFolderPathServices folderPath, ISmartMeterRepo _smartMeterRepo)
        {
            this.folderPath = folderPath;
            var envGenerator = new GenerateEnvFile(folderPath);
            envGenerator.EnvFileGenerator();
            Env.Load(folderPath.GetWattWiseFolderPath() + "\\server-side\\.env");
            _rsaPrivateKey = Env.GetString("RSA_PRIVATE_KEY");
            _messagesServices = messagesServices;
            _calculateCostClient = calculateCostClient;
            _clientCertificate = new X509Certificate2(folderPath.GetClientFolderPath() + "\\client_certificate.pfx", "a2bf39b00064f4163c868d075b35a2a28b87cf0f471021f7578f866851dc866f");
            this._smartMeterRepo = _smartMeterRepo;

        }

        // DELETE BEFORE DEPLOY
        public async Task TempStartClient()
        {
            var generateKeys = new HandleEncryption();
            var getKeys = generateKeys.GenerateKeys();

            try
            {
                using (var poller = new NetMQPoller())
                {
                    int maxClients = 12;
                    int minInterval = 1000;
                    int maxInterval = 3000;
                    var currentInterval = new Random();

                    for (int i = 0; i < maxClients; i++)
                    {
                        int clientId = i;

                        bool sslAuthenticated = false;
                        Task sslTask = Task.Run(() =>
                        {
                            try
                            {
                                Console.WriteLine(
                                    $"Client {clientId}: Initializing SSL connection");
                                using (TcpClient tcpClient = new TcpClient("localhost", 5556))
                                using (var sslStream = new SslStream(tcpClient.GetStream(), false,
                                           (sender, cert, chain, errors) => true))
                                {
                                    sslStream.AuthenticateAsClient("localhost",
                                        new X509CertificateCollection { _clientCertificate },
                                        SslProtocols.Tls12, false);
                                    if (sslStream.IsAuthenticated)
                                    {
                                        sslAuthenticated = true;
                                        Console.WriteLine(
                                            $"Client {clientId}: TLS authentication successful!");
                                    }
                                    else
                                    {
                                        Console.WriteLine(
                                            $"Client {clientId}: TLS authentication failed!");
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(
                                    $"Client {clientId}: SSL setup error - {ex.Message}");
                            }
                        });

                        sslTask.Wait();
                        if (!sslAuthenticated) continue;

                        var clientSocket = new DealerSocket();
                        clientSocket.Options.Identity =
                            Encoding.UTF8.GetBytes($"Client-{clientId}");
                        clientSocket.Connect("tcp://localhost:5555");

                        var timer = new NetMQTimer(minInterval);
                        bool awaitingResponse = false;

                        clientSocket.ReceiveReady += async (s, e) =>
                        {
                            var receivedMessage = e.Socket.ReceiveMultipartMessage();
                            Console.WriteLine(
                                $"Client {clientId} received response: {receivedMessage}");

                            var handleEncryption = new HandleEncryption();
                            var result = handleEncryption.ApplyDencryptionServer(
                                receivedMessage,
                                receivedMessage[1].Buffer,
                                receivedMessage[2].Buffer,
                                Encoding.UTF8.GetString(receivedMessage[3].Buffer),
                                Encoding.UTF8.GetString(receivedMessage[4].Buffer),
                                _rsaPrivateKey
                            );

                            awaitingResponse = false;
                            int newInterval = currentInterval.Next(minInterval, maxInterval);
                            timer.Interval = newInterval;
                            Console.WriteLine(
                                $"Client {clientId}: Next message in {newInterval} ms");
                            timer.Enable = true;


                        };

                        timer.Elapsed += (sender, e) =>
                        {
                            if (!awaitingResponse)
                            {
                                awaitingResponse = true;
                                string clientAddress = $"Client-{clientId}";

                                var genTestModel = new SmartDeviceClient
                                    { SmartMeterId = clientId };

                                var getSmartMeterId = _smartMeterRepo.GetById(clientId);

                                var modelData = _calculateCostClient.getRandomCost(genTestModel,
                                    getSmartMeterId.CustomerType);
                                var messageToServer = _messagesServices.SendReading(
                                    clientAddress,
                                    modelData,
                                    getKeys.key,
                                    getKeys.iv
                                );

                                Console.WriteLine($"Client {clientId}: Sending message...");
                                clientSocket.SendMultipartMessage(messageToServer);

                                timer.Enable = false;
                            }
                        };

                        poller.Add(clientSocket);
                        poller.Add(timer);
                    }

                    poller.RunAsync();
                    Console.ReadLine();
                    poller.Stop();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"We could not start the client, error: {e.Message}");
                throw;
            }
        }
        public async Task StartClient()
        {
            using (var electron = new NamedPipeServerStream("meter-reading"))
            {
                while (true)
                {
                    electron.WaitForConnection();
                    using (StreamReader reader = new StreamReader(electron))
                    using (StreamWriter writer = new StreamWriter(electron))
                    {
                        // Request data for electron
                        var generateKeys = new HandleEncryption();
                        var getKeys = generateKeys.GenerateKeys();

                        try
                        {
                            using (var poller = new NetMQPoller())
                            {
                                int maxClients = 12;
                                int minInterval = 1000;
                                int maxInterval = 3000;
                                var currentInterval = new Random();

                                for (int i = 0; i < maxClients; i++)
                                {
                                    int clientId = i;

                                    bool sslAuthenticated = false;
                                    Task sslTask = Task.Run(() =>
                                    {
                                        try
                                        {
                                            Console.WriteLine(
                                                $"Client {clientId}: Initializing SSL connection");
                                            using (TcpClient tcpClient = new TcpClient("localhost", 5556))
                                            using (var sslStream = new SslStream(tcpClient.GetStream(), false,
                                                       (sender, cert, chain, errors) => true))
                                            {
                                                sslStream.AuthenticateAsClient("localhost",
                                                    new X509CertificateCollection { _clientCertificate },
                                                    SslProtocols.Tls12, false);
                                                if (sslStream.IsAuthenticated)
                                                {
                                                    sslAuthenticated = true;
                                                    /*// This is for simulating the authenticate failure
                                                       if (clientId == 1)
                                                       {
                                                           throw new AuthenticationException("Simulated TLS authentication failure.");
                                                       }*/
                                                    Console.WriteLine($"Client {clientId}: TLS authentication successful!");
                                                }
                                                else
                                                {
                                                    Console.WriteLine(
                                                        $"Client {clientId}: TLS authentication failed!");
                                                }
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            Console.WriteLine($"Client {clientId}: has TLS communication problem - {ex.Message}");
                                            return;
                                        }
                                    });

                                    sslTask.Wait();
                                    if (!sslAuthenticated) continue;

                                    var clientSocket = new DealerSocket();
                                    clientSocket.Options.Identity =
                                        Encoding.UTF8.GetBytes($"Client-{clientId}");
                                    clientSocket.Connect("tcp://localhost:5555");

                                    var timer = new NetMQTimer(minInterval);
                                    bool awaitingResponse = false;

                                    clientSocket.ReceiveReady += async (s, e) =>
                                    {
                                        var receivedMessage = e.Socket.ReceiveMultipartMessage();
                                        Console.WriteLine(
                                            $"Client {clientId} received response: {receivedMessage}");

                                        var handleEncryption = new HandleEncryption();
                                        var result = handleEncryption.ApplyDencryptionServer(
                                            receivedMessage,
                                            receivedMessage[1].Buffer,
                                            receivedMessage[2].Buffer,
                                            Encoding.UTF8.GetString(receivedMessage[3].Buffer),
                                            Encoding.UTF8.GetString(receivedMessage[4].Buffer),
                                            _rsaPrivateKey
                                        );

                                        // Retun the data
                                        await writer.WriteLineAsync(result.decryptedMessage);
                                        await writer.FlushAsync();

                                        awaitingResponse = false;
                                        int newInterval = currentInterval.Next(minInterval, maxInterval);
                                        timer.Interval = newInterval;
                                        Console.WriteLine(
                                            $"Client {clientId}: Next message in {newInterval} ms");
                                        timer.Enable = true;


                                    };

                                    timer.Elapsed += (sender, e) =>
                                    {
                                        if (!awaitingResponse)
                                        {
                                            awaitingResponse = true;
                                            string clientAddress = $"Client-{clientId}";

                                            var genTestModel = new SmartDeviceClient
                                                { SmartMeterId = clientId };

                                            var getSmartMeterId = _smartMeterRepo.GetById(clientId);

                                            var modelData = _calculateCostClient.getRandomCost(genTestModel,
                                                getSmartMeterId.CustomerType);
                                            var messageToServer = _messagesServices.SendReading(
                                                clientAddress,
                                                modelData,
                                                getKeys.key,
                                                getKeys.iv
                                            );

                                            Console.WriteLine($"Client {clientId}: Sending message...");
                                            clientSocket.SendMultipartMessage(messageToServer);

                                            timer.Enable = false;
                                        }
                                    };

                                    poller.Add(clientSocket);
                                    poller.Add(timer);
                                }

                                poller.RunAsync();
                                Console.ReadLine();
                                poller.Stop();
                            }
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine($"We could not start the client, error: {e.Message}");
                            throw;
                        }
                    }
                }
            }
        }

        
        /****************** UNUSED FUNCTIONS - DELETE LATER ******************/
        private async void PublishNewData(string data)
        {
            Task getByIdPipeTask = Task.Factory.StartNew(async () =>
            {
                using (var server = new NamedPipeServerStream("meter-reading"))
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

                            // RETURN THE DATA 
                            await writer.WriteLineAsync(data);
                            await writer.FlushAsync();
                        }
                    }
                }
            }, TaskCreationOptions.LongRunning);
            
            await Task.WhenAll(getByIdPipeTask);
        }
        
        /*public async Task ElectronServerAsync()
        {
            Task getByIdPipeTask = Task.Factory.StartNew(async () =>
            {
                while (true)
                {
                    using (var server = new NamedPipeServerStream("meter-reading"))
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
                };
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
        }*/

    }

}
