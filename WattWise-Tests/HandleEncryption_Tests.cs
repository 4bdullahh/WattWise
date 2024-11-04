using System.Security.Cryptography;
using System.Text;
using client_side.Services;
using DotNetEnv;
using Moq;
using NetMQ;
using Newtonsoft.Json;
using server_side.Cryptography;
using server_side.Services.Interface;

namespace WattWise_Tests;

public class HandleEncryption_Tests
{
        private readonly HandleEncryption _encryptionHandler;
        private readonly Mock<IFolderPathServices> _folderPathMock;
        private readonly string _rsaPrivateKey = "<RSA_PRIVATE_KEY>";
        private readonly string _rsaPublicKey = "<RSA_PUBLIC_KEY>";

        public HandleEncryption_Tests()
        {
            _encryptionHandler = new HandleEncryption();
            _folderPathMock = new Mock<IFolderPathServices>();
            var rsaKeys = Cryptography.GenerateRsaKeys();
            _rsaPublicKey = rsaKeys.publicKey;
            _rsaPrivateKey = rsaKeys.privateKey;
        }

        [Fact]
        public void GenerateKeys_ShouldReturnKeyAndIv()
        {
            // Act
            var (key, iv) = _encryptionHandler.GenerateKeys();

            // Assert
            Assert.NotNull(key);
            Assert.NotNull(iv);
            Assert.Equal(32, key.Length); 
            Assert.Equal(16, iv.Length); 
        }

        [Fact]
        public void ApplyEncryptionAndDecryption_ShouldMatchOriginalData()
        {
            // Arrange
            string _rsa_public_key;
            string _rsa_private_key;
            _folderPathMock.Setup(fp => fp.GetWattWiseFolderPath()).Returns("C:\\WattWise");
            var wattWisePath = "C:\\WattWise\\server-side";
            if (!Directory.Exists(wattWisePath))
            {
                Directory.CreateDirectory(wattWisePath);
            }

            var envGenerator = new GenerateEnvFile(_folderPathMock.Object);
            envGenerator.EnvFileGenerator();
            Env.Load($"{wattWisePath}\\.env ");
            _rsa_public_key = Env.GetString("RSA_PUBLIC_KEY");
            _rsa_private_key = Env.GetString("RSA_PRIVATE_KEY");
            
            var jsonData = JsonConvert.SerializeObject(new { UserID = 1, Name = "Test User" });
            var (key, iv) = _encryptionHandler.GenerateKeys();
            var clientAddress = "Client123";
            var encryptedData = _encryptionHandler.ApplyEncryptionClient(jsonData, key, iv, _rsa_public_key);
            var receivedMessage = new NetMQMessage();
            receivedMessage.AppendEmptyFrame();
            receivedMessage.Append(Convert.ToBase64String(encryptedData.encryptedKey));
            receivedMessage.Append(Convert.ToBase64String(encryptedData.encryptedIv));
            receivedMessage.Append(encryptedData.hashJson);
            receivedMessage.Append(encryptedData.base64EncryptedData);
            
            // Decrypt
            var decryptedData = _encryptionHandler
                .ApplyDencryptionServer(receivedMessage, receivedMessage[1].Buffer, receivedMessage[2].Buffer, encryptedData.hashJson,receivedMessage[4].Buffer.ToString(), _rsa_private_key);

            // Assert
            Assert.Equal(encryptedData.hashJson, decryptedData.userHash);
            Assert.Equal(JsonConvert.SerializeObject(jsonData), decryptedData.decryptedMessage);
        }

        [Fact]
        public void ApplyEncryptionWithInvalidKey_ShouldThrowException()
        {
            // Arrange
            var jsonData = new { UserID = 1, Name = "Invalid Key Test" };
            var (key, iv) = _encryptionHandler.GenerateKeys();
            var invalidPublicKey = "<INVALID_RSA_PUBLIC_KEY>";

            // Act & Assert
            Assert.ThrowsAny<CryptographicException>(() =>
                _encryptionHandler.ApplyEncryptionClient(jsonData, key, iv, invalidPublicKey));
        }

        [Fact]
        public void ApplyDecryptionWithTamperedData_ShouldNotMatchOriginalHash()
        {
            // Arrange
            string _rsa_public_key;
            string _rsa_private_key;
            _folderPathMock.Setup(fp => fp.GetWattWiseFolderPath()).Returns("C:\\WattWise");
            var wattWisePath = "C:\\WattWise\\server-side";
            if (!Directory.Exists(wattWisePath))
            {
                Directory.CreateDirectory(wattWisePath);
            }

            var envGenerator = new GenerateEnvFile(_folderPathMock.Object);
            envGenerator.EnvFileGenerator();
            Env.Load($"{wattWisePath}\\.env ");
            _rsa_public_key = Env.GetString("RSA_PUBLIC_KEY");
            _rsa_private_key = Env.GetString("RSA_PRIVATE_KEY");

            var originalData = new { UserID = 1, Name = "Tampered Test" };
            var (key, iv) = _encryptionHandler.GenerateKeys();
            var clientAddress = "Client123";
            
            var result = _encryptionHandler.ApplyEncryptionClient(originalData, key, iv, _rsa_public_key);
            
            var originalMessage = new NetMQMessage();
            originalMessage.Append(clientAddress);
            originalMessage.AppendEmptyFrame();
            originalMessage.Append(Convert.ToBase64String(result.encryptedKey));
            originalMessage.Append(Convert.ToBase64String(result.encryptedIv));
            originalMessage.Append(result.hashJson);
            originalMessage.Append(result.base64EncryptedData);

            var tamperedMessage = new NetMQMessage();

            tamperedMessage.AppendEmptyFrame();
            tamperedMessage.Append(originalMessage[2]); 
            tamperedMessage.Append(originalMessage[3]); 
            var tamperedData = JsonConvert.SerializeObject(new { UserID = 2, Name = "Tampered User" });
            var tamperedHash = Cryptography.GenerateHash(tamperedData);
            tamperedMessage.Append(tamperedHash); 
            byte[] tamperedEncryptedData = Cryptography.AESEncrypt(tamperedData, key, iv);
            tamperedMessage.Append(Convert.ToBase64String(tamperedEncryptedData));
            
            
            // Act
            var decryptedData = _encryptionHandler
                .ApplyDencryptionServer(tamperedMessage, tamperedMessage[1].Buffer, tamperedMessage[2].Buffer,tamperedHash , tamperedMessage[4].Buffer.ToString(), _rsa_private_key);

            // Assert
            Assert.NotEqual(originalMessage[4].ConvertToString(), decryptedData.userHash);
            
            // Cleanup
            Directory.Delete(wattWisePath, true);
        }


    }