using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using NetMQ;
using NetMQ.Sockets;
using Newtonsoft.Json;
using server_side.Services.Interface;
using System.Text;
using DotNetEnv;
using server_side.Cryptography;
using server_side.Services.Models;

namespace server_side.Services
{
    public class MessageService : IMessageServices
    {
        private readonly IUserServices _userServices;
        private readonly string _rsaPrivateKey;
        private X509Certificate2 _serverCertificate;




        private readonly IFolderPathServices _folderPathServices;
        private readonly ISmartMeterServices _smartMeterServices;
        public MessageService(IFolderPathServices folderPathServices, IUserServices userServices, ISmartMeterServices smartMeterServices)

        {
            _userServices = userServices;

            string serverSideFolderPath = folderPathServices.GetServerSideFolderPath();
            _smartMeterServices = smartMeterServices;
            var envGenerator = new GenerateEnvFile();
            envGenerator.EnvFileGenerator();
            Env.Load(serverSideFolderPath + "\\.env");
            _rsaPrivateKey = Env.GetString("RSA_PRIVATE_KEY");
            _serverCertificate = new X509Certificate2(serverSideFolderPath + "\\server_certificate.pfx", "John@Muhammad@Vinny");
        }

        public void ReceiveMessageServices()
        {
            // For Vincent and Muhammad explanation
            // I am initiating on port 5556 to get a secure connection via TLS first
            TcpListener tcpListener = new TcpListener(System.Net.IPAddress.Any, 5556);
            tcpListener.Start();

            RouterSocket server = new RouterSocket();
            NetMQPoller poller = new NetMQPoller();
            server.Bind("tcp://*:5555");

           // Now I am accepting the incoming TLS connection
                Task.Factory.StartNew(() =>
                {
                    Console.WriteLine($"Server: Using certificate: {_serverCertificate.Subject}");
                    while (true)
                    {
                        try
                        {
                            var tcpClient = tcpListener.AcceptTcpClient();
                            Task.Factory.StartNew(() =>
                            {
                                using (var sslStream = new SslStream(tcpClient.GetStream(), false))
                                {
                                    try
                                    {
                                        Console.WriteLine("Server: Starting TLS handshake...");
                                        sslStream.AuthenticateAsServer(_serverCertificate, false, SslProtocols.Tls12, false);
                                        Console.WriteLine("Server: TLS handshake completed!");

                                        if (sslStream.IsAuthenticated)
                                        {
                                            Console.WriteLine("Server: TLS authentication successful!");
                                        }
                                        else
                                        {
                                            Console.WriteLine("Server: TLS authentication failed!");
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        Console.WriteLine($"Error during TLS handshake: {ex.Message}");
                                    }
                                    server.ReceiveReady += (s, e) =>
                                    {
                                        try
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

                                                object response;
                        
                                                if (decryptedMessage.Contains("UserID"))
                                                {
                                                    response = _userServices.UserOperations(decryptedMessage);
                                                }
                                                else
                                                {
                                                    response = _smartMeterServices.UpdateMeterServices(decryptedMessage);
                                                }
                                                
                                                var jsonResponse = JsonConvert.SerializeObject(response);
                                                messageToClient.Append(jsonResponse);
                                                server.SendMultipartMessage(messageToClient);
                                                Console.WriteLine($"Sending to Client: {messageToClient}");
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            Console.WriteLine($"Error handling message: {ex.Message}");
                                        }
                                    };
                                    if (!poller.IsRunning)
                                    {
                                        poller.Add(server);
                                        poller.RunAsync();
                                    }
                                }
                            }, TaskCreationOptions.LongRunning);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error accepting TCP client: {ex.Message}");
                        }
                    }
                }, TaskCreationOptions.LongRunning); 
                
                Console.ReadLine();
                poller.Stop(); 
        }
    }
}
