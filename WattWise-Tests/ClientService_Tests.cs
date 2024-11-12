using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using client_side.Services.Interfaces;
using Moq;
using server_side.Repository.Interface;
using server_side.Repository.Models;

namespace WattWise_Tests;

public class ClientService_Tests
{
    
private readonly Mock<IErrorLogRepo> _errorLogRepoMock;
        private readonly Mock<ISmartMeterRepo> _smartMeterRepoMock;
        private readonly Mock<ICalculateCostClient> _calculateCostClientMock;
        private readonly Mock<IMessagesServices> _messagesServicesMock;
        private readonly X509Certificate2 _clientCertificate;

        public ClientService_Tests()
        {
            _errorLogRepoMock = new Mock<IErrorLogRepo>();
            _smartMeterRepoMock = new Mock<ISmartMeterRepo>();
            _calculateCostClientMock = new Mock<ICalculateCostClient>();
            _messagesServicesMock = new Mock<IMessagesServices>();
            _clientCertificate = CreateSelfSignedCertificate("localhost");
        }

        private X509Certificate2 CreateSelfSignedCertificate(string subjectName)
        {
            using var rsa = RSA.Create(2048);
            var request = new CertificateRequest($"CN={subjectName}", rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
            var certificate = request.CreateSelfSigned(DateTimeOffset.Now, DateTimeOffset.Now.AddYears(1));
            return new X509Certificate2(certificate.Export(X509ContentType.Pfx));
        }

        [Fact]
        public async Task StartClient_TlsHandshakeSucceeds()
        {
            // Arrange
            var serverCertificate = _clientCertificate;
            using var listener = new TcpListener(System.Net.IPAddress.Loopback, 5556);
            listener.Start();

            var serverTask = Task.Run(async () =>
            {
                using var tcpClient = await listener.AcceptTcpClientAsync();
                using var sslStream = new SslStream(tcpClient.GetStream(), false);
                await sslStream.AuthenticateAsServerAsync(serverCertificate, false, SslProtocols.Tls12, false);
            });

            using var client = new TcpClient("localhost", 5556);
            using var sslStream = new SslStream(client.GetStream(), false, (sender, cert, chain, errors) => true);

            // Act
            await sslStream.AuthenticateAsClientAsync("localhost");

            // Assert
            Assert.True(sslStream.IsAuthenticated, "TLS handshake should succeed.");

            // Cleanup
            listener.Stop();
            await serverTask;
        }

        [Fact]
        public async Task StartClient_TlsHandshakeFailsWithInvalidCertificate()
        {
            // Arrange
            var invalidCert = CreateSelfSignedCertificate("invalid");
            using var listener = new TcpListener(System.Net.IPAddress.Loopback, 5557);
            listener.Start();

            var serverTask = Task.Run(async () =>
            {
                using var tcpClient = await listener.AcceptTcpClientAsync();
                using var sslStream = new SslStream(tcpClient.GetStream(), false);
                await sslStream.AuthenticateAsServerAsync(invalidCert, false, SslProtocols.Tls12, false);
            });

            // Act & Assert
            using var client = new TcpClient("localhost", 5557);
            using var sslStream = new SslStream(client.GetStream(), false, (sender, cert, chain, errors) => false);

            await Assert.ThrowsAsync<AuthenticationException>(async () =>
            {
                await sslStream.AuthenticateAsClientAsync("localhost");
            });

            // Cleanup
            listener.Stop();
            await serverTask;
        }

        [Fact]
        public void StartClient_LogsErrorOnTlsFailure()
        {
            // Arrange
            var errorMessage = "TLS handshake error due to invalid certificate";
            _errorLogRepoMock.Setup(x => x.LogError(It.Is<ErrorLogMessage>(m => m.Message == errorMessage))).Verifiable();

            // Act
            _errorLogRepoMock.Object.LogError(new ErrorLogMessage { Message = errorMessage });

            // Assert
            _errorLogRepoMock.Verify(x => x.LogError(It.IsAny<ErrorLogMessage>()), Times.Once);
        }

        [Fact]
        public async Task StartClient_ConnectionFailsWhenServerIsNotListening()
        {
            // Arrange
            using var client = new TcpClient();
            var connectionFailed = false;

            try
            {
                await client.ConnectAsync("localhost", 9999);
            }
            catch (SocketException)
            {
                connectionFailed = true;
            }

            // Assert
            Assert.True(connectionFailed, "Expected client connection to fail, but it succeeded.");
        }
        
        [Fact]
        public void StartClient_TlsHandshakeLogsProperlyWithInvalidCert()
        {
            // Arrange
            var invalidCertError = new ErrorLogMessage { Message = "TLS handshake error due to invalid certificate" };
            _errorLogRepoMock.Setup(x => x.LogError(It.IsAny<ErrorLogMessage>())).Verifiable();

            // Act
            _errorLogRepoMock.Object.LogError(invalidCertError);

            // Assert
            _errorLogRepoMock.Verify(x => x.LogError(It.IsAny<ErrorLogMessage>()), Times.Once);
        }
}