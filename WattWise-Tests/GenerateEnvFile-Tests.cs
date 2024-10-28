using System.IO;
using server_side.Cryptography;
using server_side.Services;
using server_side.Services.Interface;
using Xunit;
using Moq;

namespace server_side.Tests
{
    public class GenerateEnvFileTests
    {
        [Fact]
        public void LoadExistingEnvVariables_EmptyFile_ReturnsEmptyDictionary()
        {
            // Arrange
            var envFilePath = "test.env";
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
            var envFilePath = "test.env";
            var mockFolderPathServices = new Mock<IFolderPathServices>();
            mockFolderPathServices.Setup(m => m.GetWattWiseFolderPath()).Returns("test-wattwise-folder");
            var generateEnvFile = new GenerateEnvFile(mockFolderPathServices.Object);

            // Create a test file with valid content
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

        /*[Fact]
        public void LoadExistingEnvVariables_Exception_RethrowsException()
        {
            // Arrange
            var envFilePath = "non-existent-file.env";
            var mockFolderPathServices = new Mock<IFolderPathServices>();
            mockFolderPathServices.Setup(m => m.GetWattWiseFolderPath()).Returns("test-wattwise-folder");
            var generateEnvFile = new GenerateEnvFile(mockFolderPathServices.Object);

            // Act & Assert
            Assert.Throws<Exception>(() => generateEnvFile.LoadExistingEnvVariables(envFilePath));
        }*/
    }
}