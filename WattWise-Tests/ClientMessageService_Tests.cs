using client_side.Services;
using DotNetEnv;
using Moq;
using NetMQ;
using Newtonsoft.Json;
using server_side.Cryptography;
using server_side.Repository.Interface;
using server_side.Repository.Models;
using server_side.Services.Interface;

namespace WattWise_Tests;

public class ClientMessageService_Tests
{
     private readonly MessagesServices _messagesServices;
        private readonly Mock<IFolderPathServices> _folderPathMock;
        private readonly Mock<IErrorLogRepo> _errorLogRepoMock;
        private readonly string _rsaPublicKey;
        private readonly string _rsaPrivateKey;
        private readonly HandleEncryption _encryptionHandler;

        public ClientMessageService_Tests()
        {
            _folderPathMock = new Mock<IFolderPathServices>();
            _errorLogRepoMock = new Mock<IErrorLogRepo>();
            _encryptionHandler = new HandleEncryption();

            var rsaKeys = Cryptography.GenerateRsaKeys();
            _rsaPublicKey = rsaKeys.publicKey;
            _rsaPrivateKey = rsaKeys.privateKey;
            _folderPathMock.Setup(x => x.GetWattWiseFolderPath()).Returns(Path.Combine(Path.GetTempPath(), "TestEnvFolder"));

            _messagesServices = new MessagesServices(_folderPathMock.Object, _errorLogRepoMock.Object);
            _encryptionHandler = new HandleEncryption();
        }

     [Fact]
    public void SendReading_ShouldReturnMessage_WhenDataIsValid()
    {
        // Arrange
        var clientAddress = "Client123";
        var modelData = new { UserID = 1, Name = "Test User" };
        var folderPathMock = new Mock<IFolderPathServices>();
        folderPathMock.Setup(fp => fp.GetWattWiseFolderPath()).Returns("C:\\WattWise");

        var envGenerator = new GenerateEnvFile(folderPathMock.Object);
        envGenerator.EnvFileGenerator();
        Env.Load("C:\\WattWise\\server-side\\.env");

        var rsaPublicKey = Env.GetString("RSA_PUBLIC_KEY");

        var errorLogRepoMock = new Mock<IErrorLogRepo>();
        var messagesServices = new MessagesServices(folderPathMock.Object, errorLogRepoMock.Object);

        // Generate encryption keys
        var handleEncryption = new HandleEncryption();
        var (key, iv) = handleEncryption.GenerateKeys();

        var encryptedData = handleEncryption.ApplyEncryptionClient(modelData, key, iv, rsaPublicKey);
        var expectedEncryptedMessage = new NetMQMessage();
        expectedEncryptedMessage.Append(clientAddress);
        expectedEncryptedMessage.AppendEmptyFrame();
        expectedEncryptedMessage.Append(Convert.ToBase64String(encryptedData.encryptedKey));
        expectedEncryptedMessage.Append(Convert.ToBase64String(encryptedData.encryptedIv));
        expectedEncryptedMessage.Append(encryptedData.hashJson);
        expectedEncryptedMessage.Append(encryptedData.base64EncryptedData);

        // Act
        var result = messagesServices.SendReading(clientAddress, modelData, key, iv);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(clientAddress, result[0].ConvertToString());
        Assert.Equal(6, result.FrameCount);
    }

        [Fact]
    public void SendReading_ShouldThrowException_WhenEncryptionFails()
    {
        // Arrange
        var clientAddress = "Client123";
        var modelData = new { UserID = 1, Name = "Test User" };
        var key = new byte[32]; // Invalid key size
        var iv = new byte[16];  // Invalid iv size

        // Act & Assert
        Assert.ThrowsAny<Exception>(() => _messagesServices.SendReading(clientAddress, modelData, key, iv));
    }

    [Fact]
    public void SendReading_ShouldLogError_WhenExceptionIsThrown()
    {
        // Arrange
        var clientAddress = "Client123";
        var modelData = new { UserID = 1, Name = "Test User" };
        var key = new byte[32]; // Invalid key
        var iv = new byte[16]; // Invalid iv

        // Mock the error logging behavior
        _errorLogRepoMock.Setup(x => x.LogError(It.IsAny<ErrorLogMessage>())).Verifiable();

        // Act
        try
        {
            _messagesServices.SendReading(clientAddress, modelData, key, iv);
        }
        catch (Exception)
        {
            // Assert that an error was logged when an exception occurred
            _errorLogRepoMock.Verify(x => x.LogError(It.Is<ErrorLogMessage>(m => m.Message.Contains("Message did not sent to server"))), Times.Once);
        }
    }
    [Fact]
    public void SendReading_ShouldLogError_WhenDecryptionFails()
    {
        // Arrange
        var clientAddress = "Client123";
        var modelData = new { UserID = 1, Name = "Test User" };
        var key = new byte[32]; // Invalid key size
        var iv = new byte[16];  // Invalid iv size

        // Set up mock logging for error verification
        _errorLogRepoMock.Setup(x => x.LogError(It.IsAny<ErrorLogMessage>())).Verifiable();

        // Act & Assert
        Assert.ThrowsAny<Exception>(() => _messagesServices.SendReading(clientAddress, modelData, key, iv));

        // Verify that the error log contains the expected decryption error
        _errorLogRepoMock.Verify(x => x.LogError(It.Is<ErrorLogMessage>(m => m.Message.Contains("Message did not sent to server"))), Times.Once);
    }
    }