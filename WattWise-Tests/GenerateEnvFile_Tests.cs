using System.IO;
using server_side.Cryptography;
using server_side.Services;
using server_side.Services.Interface;
using Xunit;
using Moq;

namespace server_side.Tests
{
    public class GenerateEnvFile_Tests
    {
        
        private readonly Mock<IFolderPathServices> _mockFolderPathServices;
        private readonly GenerateEnvFile _generateEnvFile;

        public GenerateEnvFile_Tests()
        {
            _mockFolderPathServices = new Mock<IFolderPathServices>();
            _generateEnvFile = new GenerateEnvFile(_mockFolderPathServices.Object);
        }
        
        [Fact]
        public void LoadExistingEnvVariables_EmptyFile_ReturnsEmptyDictionary()
        {
            // Arrange
            var envFilePath = ".env";
            var mockFolderPathServices = new Mock<IFolderPathServices>();
            mockFolderPathServices.Setup(m => m.GetWattWiseFolderPath()).Returns("test-wattwise-folder");
            var generateEnvFile = new GenerateEnvFile(mockFolderPathServices.Object);

            // Act
            var envVariables = generateEnvFile.LoadExistingEnvVariables(envFilePath);

            // Assert
            Assert.Empty(envVariables);
        }

        [Fact]
        public void LoadExistingEnvVariables_ValidFile_ReturnsDictionary()
        {
            // Arrange
            var envFilePath = ".env";
            var mockFolderPathServices = new Mock<IFolderPathServices>();
            mockFolderPathServices.Setup(m => m.GetWattWiseFolderPath()).Returns("test-wattwise-folder");
            var generateEnvFile = new GenerateEnvFile(mockFolderPathServices.Object);

            File.WriteAllLines(envFilePath, new string[] {
                "KEY1=VALUE1",
                "# Comment line",
                "KEY2=VALUE2"
            });

            // Act
            var envVariables = generateEnvFile.LoadExistingEnvVariables(envFilePath);

            // Assert
            Assert.Equal(2, envVariables.Count);
            Assert.True(envVariables.ContainsKey("KEY1"));
            Assert.Equal("VALUE1", envVariables["KEY1"]);
            Assert.True(envVariables.ContainsKey("KEY2"));
            Assert.Equal("VALUE2", envVariables["KEY2"]);

            // Cleanup (delete test file)
            File.Delete(envFilePath);
        }
        [Fact]
        public void EnvFileGenerator_ShouldGenerateKeys_WhenKeysDoNotExist()
        {
            // Arrange
            var tempPath = Path.Combine(Path.GetTempPath(), "TestEnvFolder", "server-side");
            Directory.CreateDirectory(tempPath);
            var fakePath = Path.Combine(tempPath, ".env");
    
            _mockFolderPathServices.Setup(x => x.GetWattWiseFolderPath()).Returns(Path.Combine(Path.GetTempPath(), "TestEnvFolder"));
            var rsaKeys = ("publicKey", "privateKey");
            
            // For future references: This is for mock RSA Keys
            File.WriteAllLines(fakePath, new[]
            {
                "RSA_PUBLIC_KEY=publicKey",
                "RSA_PRIVATE_KEY=privateKey"
            });
            
            // Act
            _generateEnvFile.EnvFileGenerator();

            // Assert
            var envVariables = _generateEnvFile.LoadExistingEnvVariables(fakePath);
            Assert.Equal("publicKey", envVariables["RSA_PUBLIC_KEY"]);
            Assert.Equal("privateKey", envVariables["RSA_PRIVATE_KEY"]);

            // Here I am cleaning up the temporary file
            Directory.Delete(tempPath, true);
        }

        [Fact]
        public void LoadExistingEnvVariables_ShouldReturnCorrectDictionary_WhenFileExists()
        {
            // Arrange
            string envFilePath = ".env";
            File.WriteAllLines(envFilePath, new[] { "KEY1=VALUE1", "KEY2=VALUE2" });

            // Act
            var result = _generateEnvFile.LoadExistingEnvVariables(envFilePath);

            // Assert
            Assert.Equal("VALUE1", result["KEY1"]);
            Assert.Equal("VALUE2", result["KEY2"]);
        }

        [Fact]
        public void WriteEnvVariablesToFile_ShouldWriteCorrectly()
        {
            // Arrange
            string envFilePath = "test.env";
            var envVariables = new Dictionary<string, string>
            {
                { "KEY1", "VALUE1" },
                { "KEY2", "VALUE2" }
            };

            // Act
            _generateEnvFile.WriteEnvVariablesToFile(envFilePath, envVariables);

            // Assert
            var lines = File.ReadAllLines(envFilePath);
            Assert.Contains("KEY1=VALUE1", lines);
            Assert.Contains("KEY2=VALUE2", lines);
        }
    }
}