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
using server_side.Repository.Interface;
using server_side.Repository.Models;
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
        private readonly IErrorLogRepo _errorLogRepo;
        private readonly ErrorLogMessage errorMessage;
        public ClientServices(IMessagesServices messagesServices, ICalculateCostClient calculateCostClient, IFolderPathServices folderPath, ISmartMeterRepo _smartMeterRepo, IErrorLogRepo errorLogRepo)
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
            _errorLogRepo = errorLogRepo;
            errorMessage = new ErrorLogMessage();
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
                    int maxClients = 15;
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
                                        // This is for simulating the authenticate failure
                                              /*if (clientId == 1)
                                              {
                                                  errorMessage.Message = $"Client: {clientId} Simulated TLS authentication failure : {DateTime.UtcNow}";
                                                  tcpClient.Close();
                                                  sslStream.Close();
                                                  throw new AuthenticationException($"Simulated TLS authentication failure : {DateTime.UtcNow}");
                                              }*/
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
                                // Uncomment when simulating connection error
                                _errorLogRepo.LogError(errorMessage);
                                Console.WriteLine($"Client {clientId}: has TLS communication problem - {ex.Message} : {DateTime.UtcNow}");
                                // Uncomment when simulating connection error
                              //  return;
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
        /*  Code Documentation:
            This Method is responsible for starting the client and includes: 
                TLS Security
                Receive/Send Encrypted and Decrypted Messages
                Connection with Electron
       */
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
                                                           errorMessage.Message = $"Client: {clientId} Simulated TLS authentication failure : {DateTime.UtcNow}";
                                                           tcpClient.Close();
                                                           sslStream.Close();
                                                           throw new AuthenticationException("Simulated TLS authentication failure : {DateTime.UtcNow}");
                                                       }*/
                                                    Console.WriteLine($"Client {clientId}: TLS authentication successful!");
                                                }
                                                else
                                                {
                                                    Console.WriteLine($"Client {clientId}: TLS authentication failed!");
                                                }
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            // Uncomment when simulating connection error
                                            // _errorLogRepo.LogError(errorMessage);
                                            Console.WriteLine($"Client {clientId}: has TLS communication problem - {ex.Message} : {DateTime.UtcNow}");
                                            // Uncomment when simulating connection error
                                            //return;
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
    }
}
