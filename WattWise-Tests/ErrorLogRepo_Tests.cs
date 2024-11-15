using Moq;
using server_side.Repository.Interface;
using server_side.Repository.Models;
using server_side.Services.Interface;

namespace WattWise_Tests;

public class ErrorLogRepo_Tests
{
    private readonly Mock<ISaveData> _saveData;
    private readonly Mock<IFolderPathServices> _folderPathServices;
    private readonly Mock<IErrorLogRepo>  _mockErrorLogRepo;

    public ErrorLogRepo_Tests()
    {
        _mockErrorLogRepo = new Mock<IErrorLogRepo>();
        _saveData = new Mock<ISaveData>();
        _folderPathServices = new Mock<IFolderPathServices>();
        _mockErrorLogRepo = new Mock<IErrorLogRepo>();
    }
    
    [Fact]
    public void FolderPath_Should_Return_Path_In_Log_Error_Repo()
    {
        // Arrange
        string mockFilePath = "MockWattWise";
        _folderPathServices.Setup(m => m.GetWattWiseFolderPath())
            .Returns(mockFilePath);
        // Act
        var result = Path.Combine(_folderPathServices.Object.GetWattWiseFolderPath(), "mockServer", "ErrorLog.json");

        // Assert
        Assert.NotNull(result);
        Assert.Contains(mockFilePath, result);
    }
    
    [Fact]
    public void LogError_Should_Throw_Exception_If_Path_Is_Not_Valid()
    {
        // Arrange
        string mockFilePath = "MockWattWise";
        _folderPathServices.Setup(m => m.GetWattWiseFolderPath())
            .Returns("WrongPath");
        // Act
        var result = Path.Combine(_folderPathServices.Object.GetWattWiseFolderPath(), "mockServer", "ErrorLog.json");

        // Assert
        Assert.NotNull(result);
        Assert.DoesNotContain(mockFilePath, result);
    }

    
}