using System.Net.Security;
using Moq;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using server_side.Cryptography;
using server_side.Repository.Interface;
using server_side.Repository.Models;
using server_side.Services;
using server_side.Services.Interface;
using server_side.Services.Models;

namespace server_side.Tests
{
    public class ServerMessageService_Tests
    {
        private readonly Mock<TcpListener> _tcpListenerMock;
        private readonly Mock<IUserServices> _userServicesMock;
        private readonly X509Certificate2 _serverCertificate;
        private readonly Mock<IFolderPathServices> _folderPathServicesMock;
        private readonly Mock<ISmartMeterServices> _smartMeterServicesMock;
        private readonly Mock<IErrorLogRepo> _errorLogRepoMock;
        private MessageService _messageService;
        private string _tempPath;
        private string _fakeCertPath;
        private TcpListener _tcpListener;


        public ServerMessageService_Tests()
        {
            
            _userServicesMock = new Mock<IUserServices>();
            _folderPathServicesMock = new Mock<IFolderPathServices>();
            _smartMeterServicesMock = new Mock<ISmartMeterServices>();
            _errorLogRepoMock = new Mock<IErrorLogRepo>();
           // _pollerMock = new Mock<NetMQPoller>();

            // Setup temporary paths
            var tempPath = Path.Combine(Path.GetTempPath(), "TestEnvFolder", "server-side");
            Directory.CreateDirectory(tempPath);
            var fakePath = Path.Combine(tempPath, ".env");
    
            _folderPathServicesMock.Setup(x => x.GetWattWiseFolderPath()).Returns(Path.Combine(Path.GetTempPath(), "TestEnvFolder"));
            var envFilePath = ".env";
            var mockFolderPathServices = new Mock<IFolderPathServices>();
            mockFolderPathServices.Setup(m => m.GetWattWiseFolderPath()).Returns("test-wattwise-folder");
            File.WriteAllLines(fakePath, new[]
            {
                "RSA_PUBLIC_KEY=publicKey",
                "RSA_PRIVATE_KEY=privateKey"
            });
        
            // Generate a self-signed certificate file
            _fakeCertPath = Path.Combine(tempPath, "server_certificate.pfx");
            _serverCertificate = new X509Certificate2(_fakeCertPath, "a2bf39b00064f4163c868d075b35a2a28b87cf0f471021f7578f866851dc866f");
            CreateSelfSignedCertificate(_fakeCertPath, "a2bf39b00064f4163c868d075b35a2a28b87cf0f471021f7578f866851dc866f");
            _folderPathServicesMock.Setup(x => x.GetServerSideFolderPath()).Returns(tempPath);

            // Initialize the MessageService instance with mocks
            _messageService = new MessageService(
                _folderPathServicesMock.Object,
                _userServicesMock.Object,
                _smartMeterServicesMock.Object,
                _errorLogRepoMock.Object
            );
        }
        private void CreateSelfSignedCertificate(string certPath, string password)
        {
            using (var rsa = RSA.Create(2048))
            {
                var request = new CertificateRequest(
                    "CN=TestCertificate",
                    rsa,
                    HashAlgorithmName.SHA256,
                    RSASignaturePadding.Pkcs1
                );

                // Self-sign the certificate
                var certificate = request.CreateSelfSigned(DateTimeOffset.Now, DateTimeOffset.Now.AddYears(1));

                // Export the certificate as a PFX file
                byte[] certData = certificate.Export(X509ContentType.Pfx, password);
                File.WriteAllBytes(certPath, certData);
            }
        }
        
        [Fact]
        public void ServerCertificatePathIsSetCorrectly()
        {
            // Arrange
            var tempPath = Path.Combine(Path.GetTempPath(), "TestEnvFolder", "server-side");
            Directory.CreateDirectory(tempPath);
            _fakeCertPath = Path.Combine(tempPath, "server_certificate.pfx");
            CreateSelfSignedCertificate(_fakeCertPath, "a2bf39b00064f4163c868d075b35a2a28b87cf0f471021f7578f866851dc866f");
            _folderPathServicesMock.Setup(x => x.GetServerSideFolderPath()).Returns(tempPath);
            var certFolder = $"{tempPath}\\server_certificate.pfx";
            
            //Act
            var normalizedExpectedPath = Path.GetFullPath(certFolder);
            var normalizedActualPath = Path.GetFullPath(_fakeCertPath);
            //Assert
            Assert.Equal(normalizedExpectedPath, normalizedActualPath);
        }
        
        [Fact]
    public async Task TestTlsConnectionWithReceiveMessageServices()
    {
        // Arrange: Start the server in a background task
        _tcpListener = new TcpListener(System.Net.IPAddress.Any, 5556);
        _tcpListener.Start();

        // Start the server in a background task (server logic)
        var serverTask = Task.Run(() => _messageService.ReceiveMessageServices());

        // Wait for the server to initialize (give it time to bind to the port)
        await Task.Delay(500); // Allow the server to start and listen

        // Act: Create the TCP client
        var tcpClient = new TcpClient("localhost", 5556);

        // Use a custom certificate validation callback to bypass certificate validation
        using (var sslStream = new SslStream(tcpClient.GetStream(), false, 
            new RemoteCertificateValidationCallback((sender, certificate, chain, sslPolicyErrors) => true)))  // Bypass validation
        {
            try
            {
                // Start the TLS handshake (client-side)
                await sslStream.AuthenticateAsClientAsync("localhost");
                Console.WriteLine("Client: TLS handshake completed!");

                // Send data to the server (encrypted over TLS)
                var message = "Hello, server!";
                var messageBytes = Encoding.UTF8.GetBytes(message);
                await sslStream.WriteAsync(messageBytes, 0, messageBytes.Length);
                await sslStream.FlushAsync();
                Console.WriteLine("Client: Message sent to server");

                // Wait for a response (optional, depending on your server logic)
                var buffer = new byte[1024];
                var bytesRead = await sslStream.ReadAsync(buffer, 0, buffer.Length);
                var responseMessage = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                Console.WriteLine($"Client: Received response: {responseMessage}");

                // Assert the server response or other checks
                // Example: Assert.Equal("expected response", responseMessage);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Client: Exception during TLS handshake: {ex.Message}");
                throw;  // Rethrow for test failure
            }
        }

        // Clean up: Close the client and stop the server
        tcpClient.Close();
        _tcpListener.Stop();
        serverTask.Dispose();
    }
    
    }
}
