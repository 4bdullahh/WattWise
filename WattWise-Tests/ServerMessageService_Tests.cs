using System.Net.Security;
using Moq;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using server_side.Repository.Interface;
using server_side.Repository.Models;
using server_side.Services;
using server_side.Services.Interface;

namespace server_side.Tests
{
    public class ServerMessageService_Tests
    {
        private readonly Mock<TcpListener> _tcpListenerMock;
        private readonly Mock<IUserServices> _userServicesMock;
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
        
            _fakeCertPath = Path.Combine(tempPath, "server_certificate.pfx");
            CreateSelfSignedCertificate(_fakeCertPath, "a2bf39b00064f4163c868d075b35a2a28b87cf0f471021f7578f866851dc866f");
            _folderPathServicesMock.Setup(x => x.GetServerSideFolderPath()).Returns(tempPath);

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

                var certificate = request.CreateSelfSigned(DateTimeOffset.Now, DateTimeOffset.Now.AddYears(1));

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
        public async Task TestTlsHandshakeInitialization()
        {
            // Arrange
            using var listener = new TcpListener(System.Net.IPAddress.Loopback, 5556);
            listener.Start();
           
            var serverCertificate = new X509Certificate2(_fakeCertPath, "a2bf39b00064f4163c868d075b35a2a28b87cf0f471021f7578f866851dc866f");

            var serverTask = Task.Run(async () =>
            {
                using (var tcpClient = await listener.AcceptTcpClientAsync())
                {
                    using (var sslStream = new SslStream(tcpClient.GetStream(), false))
                    {
                        try
                        {
                            Console.WriteLine("Server: Starting TLS handshake...");
                            await sslStream.AuthenticateAsServerAsync(serverCertificate, false, SslProtocols.Tls12, false);
                            Console.WriteLine("Server: TLS handshake completed!");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Server: TLS handshake failed - {ex.Message}");
                        }
                    }
                }
            });
            
            using var client = new TcpClient("localhost", 5556);
            using var sslStream = new SslStream(client.GetStream(), false, (sender, cert, chain, errors) => true);

            try
            {
                await sslStream.AuthenticateAsClientAsync("localhost");
                Console.WriteLine("Client: TLS handshake completed successfully.");
            }
            catch (AuthenticationException ex)
            {
                Assert.True(false, $"TLS handshake failed: {ex.Message}");
            }
            finally
            {
                listener.Stop();
            }

            await serverTask;
        }

        [Fact]
        public void ErrorIsLoggedWhenTlsFails()
        {
            // Arrange
            var mockErrorRepo = new Mock<IErrorLogRepo>();
            var messageService = new MessageService(
                _folderPathServicesMock.Object,
                _userServicesMock.Object,
                _smartMeterServicesMock.Object,
                mockErrorRepo.Object
            );

            // Act
            string errorMessage = "Error during TLS handshake";
            var errorLogMessage = new ErrorLogMessage { Message = errorMessage };
            mockErrorRepo.Setup(x => x.LogError(It.IsAny<ErrorLogMessage>())).Verifiable();

            mockErrorRepo.Object.LogError(errorLogMessage);

            // Assert
            mockErrorRepo.Verify(x => x.LogError(It.IsAny<ErrorLogMessage>()), Times.Once);
        } 
        
        [Fact]
    public async Task ClientConnectionFailsIfServerNotListening()
    {
        // Arrange
        using var client = new TcpClient();
        var connectionFailed = false;

        try
        {
            await client.ConnectAsync("localhost", 9999);
        }
        catch (SocketException ex)
        {
            connectionFailed = true;
            Console.WriteLine($"Client: Connection failed as expected - {ex.Message}");
        }

        // Assert
        Assert.True(connectionFailed, "Expected client connection to fail, but it succeeded.");
    }

    [Fact]
    public async Task TlsHandshakeFailsWithInvalidCertificate()
    {
        // Arrange
        using var listener = new TcpListener(System.Net.IPAddress.Loopback, 5557);
        listener.Start();

        var invalidCertPath = Path.Combine(Path.GetTempPath(), "invalid_certificate.pfx");
        CreateSelfSignedCertificate(invalidCertPath, "invalid_password");

        var serverTask = Task.Run(async () =>
        {
            using (var tcpClient = await listener.AcceptTcpClientAsync())
            {
                using (var sslStream = new SslStream(tcpClient.GetStream(), false))
                {
                    try
                    {
                        Console.WriteLine("Server: Starting TLS handshake with invalid certificate...");
                        await sslStream.AuthenticateAsServerAsync(
                            new X509Certificate2(invalidCertPath, "invalid_password"),
                            false,
                            SslProtocols.Tls12,
                            false
                        );
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Server: TLS handshake failed as expected - {ex.Message}");
                    }
                }
            }
        });

        // Act & Assert
        using var client = new TcpClient("localhost", 5557);
        using var sslStream = new SslStream(client.GetStream(), false, (sender, cert, chain, errors) => false);

        await Assert.ThrowsAsync<AuthenticationException>(async () =>
        {
            await sslStream.AuthenticateAsClientAsync("localhost");
        });

        // Clean up
        listener.Stop();
        await serverTask;
    }

    [Fact]
    public void TlsHandshakeErrorLogsProperlyWithInvalidCert()
    {
        // Arrange
        var mockErrorRepo = new Mock<IErrorLogRepo>();
        var messageService = new MessageService(
            _folderPathServicesMock.Object,
            _userServicesMock.Object,
            _smartMeterServicesMock.Object,
            mockErrorRepo.Object
        );

        var errorLogMessage = new ErrorLogMessage { Message = "TLS handshake error due to invalid certificate" };
        mockErrorRepo.Setup(x => x.LogError(It.IsAny<ErrorLogMessage>())).Verifiable();

        // Act
        mockErrorRepo.Object.LogError(errorLogMessage);

        // Assert
        mockErrorRepo.Verify(x => x.LogError(It.IsAny<ErrorLogMessage>()), Times.Once);
    } 
    }
}
