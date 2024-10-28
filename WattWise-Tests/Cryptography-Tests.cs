using System;
using System.Security.Cryptography;
using System.Text;
using Xunit;
using server_side.Cryptography;

namespace CryptographyTests
{
    public class CryptographyTests
    {
        
        [Fact]
        public void GenerateHash_ShouldReturnConsistentHashForSameInput()
        {
            // Arrange
            string input = "TestString";

            // Act
            string hash1 = Cryptography.GenerateHash(input);
            string hash2 = Cryptography.GenerateHash(input);

            // Assert
            Assert.Equal(hash1, hash2);
        }

        [Fact]
        public void GenerateHash_ShouldReturnDifferentHashesForDifferentInputs()
        {
            // Arrange
            string input1 = "TestString1";
            string input2 = "TestString2";

            // Act
            string hash1 = Cryptography.GenerateHash(input1);
            string hash2 = Cryptography.GenerateHash(input2);

            // Assert
            Assert.NotEqual(hash1, hash2);
        }

        [Fact]
        public void GenerateRsaKeys_ShouldGenerateNonNullKeys()
        {
            // Arrange & Act
            var (publicKey, privateKey) = Cryptography.GenerateRsaKeys();

            // Assert
            Assert.False(string.IsNullOrWhiteSpace(publicKey));
            Assert.False(string.IsNullOrWhiteSpace(privateKey));
        }

        [Fact]
        public void RSAEncryptAndDecrypt_ShouldReturnOriginalData()
        {
            // Arrange
            var (publicKey, privateKey) = Cryptography.GenerateRsaKeys();
            byte[] data = Encoding.UTF8.GetBytes("Sample data");

            // Act
            byte[] encryptedData = Cryptography.RSAEncrypt(publicKey, data);
            byte[] decryptedData = Cryptography.RSADecrypt(privateKey, encryptedData);
            string result = Encoding.UTF8.GetString(decryptedData);

            // Assert
            Assert.Equal("Sample data", result);
        }

        [Fact]
        public void AESEncryptAndDecrypt_ShouldReturnOriginalText()
        {
            // Arrange
            string originalText = "Sensitive data";
            using (Aes aes = Aes.Create())
            {
                byte[] key = aes.Key;
                byte[] iv = aes.IV;

                // Act
                byte[] encryptedData = Cryptography.AESEncrypt(originalText, key, iv);
                string decryptedText = Cryptography.AESDecrypt(encryptedData, key, iv);

                // Assert
                Assert.Equal(originalText, decryptedText);
            }
        }
        [Fact]
        public void RSAEncrypt_Decrypt_WithValidKeys_ShouldSucceed()
        {
            // Arrange
            var (publicKey, privateKey) = Cryptography.GenerateRsaKeys();
            var originalData = Encoding.UTF8.GetBytes("Hello, World!");

            // Act
            var encryptedData = Cryptography.RSAEncrypt(publicKey, originalData);
            var decryptedData = Cryptography.RSADecrypt(privateKey, encryptedData);

            // Assert
            Assert.Equal(originalData, decryptedData);
        }
        [Fact]
        public void RSADecrypt_WithInvalidPrivateKey_ShouldThrowException()
        {
            // Arrange
            var (publicKey, _) = Cryptography.GenerateRsaKeys();
            var (invalidPublicKey, _) = Cryptography.GenerateRsaKeys(); 
            var originalData = Encoding.UTF8.GetBytes("Hello, World!");
            var encryptedData = Cryptography.RSAEncrypt(publicKey, originalData);

            // Act
            var exception = Assert.Throws<CryptographicException>(() => 
            {
                Cryptography.RSADecrypt(invalidPublicKey, encryptedData);
            });
            // Assert
            Assert.IsType<CryptographicException>(exception);
            Assert.Contains("Key does not exist", exception.Message, StringComparison.OrdinalIgnoreCase);        }
        
        [Fact]
        public void RSAEncrypt_WithInvalidPublicKey_ShouldThrowException()
        {
            // Arrange
            var originalData = Encoding.UTF8.GetBytes("Hello, World!");
            string invalidPublicKey = "<InvalidKey></InvalidKey>";

            // Act 
            var exception = Assert.Throws<CryptographicException>(() => 
            {
                Cryptography.RSAEncrypt(invalidPublicKey, originalData);
            });
            //Assert
            Assert.IsType<CryptographicException>(exception);
            Assert.Contains("Input string does not contain a valid encoding", exception.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void RSADecrypt_WithWrongKey_ShouldThrowException()
        {
            // Arrange
            var (publicKey, privateKey) = Cryptography.GenerateRsaKeys();
            var (wrongPublicKey, wrongPrivateKey) = Cryptography.GenerateRsaKeys();
            var originalData = Encoding.UTF8.GetBytes("Hello, World!");
            var encryptedData = Cryptography.RSAEncrypt(publicKey, originalData);

            // Act 
            var exception = Assert.Throws<CryptographicException>(() => 
            {
                Cryptography.RSADecrypt(wrongPrivateKey, encryptedData);
            });
            
            // Assert
            Assert.IsType<CryptographicException>(exception);
            Assert.Contains("parameter is incorrect", exception.Message, StringComparison.OrdinalIgnoreCase);
        }
    }
}
