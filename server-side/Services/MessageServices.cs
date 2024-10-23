using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography;
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
        private readonly string _rsa_public_key;
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
            _rsa_public_key = Env.GetString("RSA_PUBLIC_KEY");
            _serverCertificate = new X509Certificate2(serverSideFolderPath + "\\server_certificate.pfx", "a2bf39b00064f4163c868d075b35a2a28b87cf0f471021f7578f866851dc866f");
        }

        public void ReceiveMessageServices()
        {
    
            TcpListener tcpListener = new TcpListener(System.Net.IPAddress.Any, 5556);
            tcpListener.Start();

            RouterSocket server = new RouterSocket();
            NetMQPoller poller = new NetMQPoller();
            server.Bind("tcp://*:5555");

                Task.Factory.StartNew(() =>
                {
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
                                            var handleEncryption = new HandleEncryption();
                                            var result = handleEncryption.ApplyDencryption(recievedMessage, recievedMessage[3].Buffer, recievedMessage[4].Buffer,
                                                Encoding.UTF8.GetString(recievedMessage[5].Buffer), Encoding.UTF8.GetString(recievedMessage[6].Buffer), _rsaPrivateKey);
                                            
                                            //This is for test when the data is temperaded
                                            /*
                                            UserData tempered = JsonConvert.DeserializeObject<UserData>(result.decryptedMessage);
                                            tempered.UserID = 1000;
                                            var temperedJson = JsonConvert.SerializeObject(tempered);
                                            string result.userHash = Cryptography.Cryptography.GenerateHash(temperedJson);*/

                                            if (result.userHash != result.receivedHash)
                                            {
                                                Console.WriteLine("Hash doesn't match");
                                            }
                                            else
                                            {
                                              
                                                var messageToClient = new NetMQMessage();
                                                messageToClient.Append(clientAddress);
                                                messageToClient.AppendEmptyFrame();
                                                var generateKeys = new HandleEncryption();
                                                var getKeys = generateKeys.GenerateKeys();
                                                var response = _userServices.UserOperations(result.decryptedMessage);
                                                var encryptMessage = new HandleEncryption();
                                                var jsonResponse = JsonConvert.SerializeObject(response);
                                                var encryptedResponse = encryptMessage.ApplyEncryptionServer(jsonResponse, getKeys.key, getKeys.iv, _rsa_public_key);
                                                messageToClient.Append(Convert.ToBase64String(encryptedResponse.encryptedKey));
                                                messageToClient.Append(Convert.ToBase64String(encryptedResponse.encryptedIv));
                                                messageToClient.Append(encryptedResponse.hashJson);
                                                messageToClient.Append(encryptedResponse.base64EncryptedData);
                                                
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
