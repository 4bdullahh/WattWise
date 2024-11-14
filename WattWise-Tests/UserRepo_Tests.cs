using FizzWare.NBuilder.Extensions;
using Moq;
using Newtonsoft.Json;
using server_side.Repository;
using server_side.Repository.Interface;
using server_side.Repository.Models;
using server_side.Services.Interface;
using Xunit.Sdk;

public class UserRepo_Tests
{
    private readonly Mock<ISaveData> _saveData;
    private readonly Mock<IFolderPathServices> _folderPathServices;
    private readonly Mock<ISmartMeterRepo> _mockSmartMeterRepo;
    private readonly Mock<IUserMessageRepo> _mockUserMessageRepo;
    private readonly Mock<IErrorLogRepo>  _mockErrorLogRepo;

    public UserRepo_Tests()
    {
        _mockErrorLogRepo = new Mock<IErrorLogRepo>();
        _saveData = new Mock<ISaveData>();
        _folderPathServices = new Mock<IFolderPathServices>();
        _mockSmartMeterRepo = new Mock<ISmartMeterRepo>();
        _mockUserMessageRepo = new Mock<IUserMessageRepo>();
        _mockErrorLogRepo = new Mock<IErrorLogRepo>();
        
    }

    [Theory]
    [InlineData(
        1001
        )]
    public void GetById_Should_Return_int(int userId)
    {
        // Arrange
        var expectedUser = new UserData
        {
            UserID = userId,
            UserEmail = "dummy1@example.com",
            CostPerKwh = 0.24,
        };
    
        _mockUserMessageRepo.Setup(repo => repo.GetById(userId)).Returns(expectedUser);

        // Act
        var result = _mockUserMessageRepo.Object.GetById(userId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedUser.UserID, result.UserID);
    }
    
    [Theory]
    [InlineData(
        1001,
        4
    )]
    public void Repo_Should_Return_Same_UserID_And_SmartMeterID(int userId, int deviceId)
    {
        // Arrange
        var expectedUser = new UserData
        {
            UserID = userId,
            UserEmail = "dummy1@example.com",
            CostPerKwh = 0.24,
            SmartMeterId = deviceId
        };
        var expectedDevice = new SmartDevice
        {
            SmartMeterId = deviceId,
            CustomerType = "Small Household",
            CostPerKwh = 0.24,
            StandingCharge = 0.60,
            UserData = expectedUser
        };
        _mockUserMessageRepo.Setup(repo => repo.UpdateUserData(expectedUser)).Returns(expectedUser).Verifiable();
        _mockSmartMeterRepo.Setup(repo => repo.GetById(expectedUser.UserID)).Returns(expectedDevice).Verifiable();
        _saveData.Setup(saveData => saveData.ListToJson(It.IsAny<SmartDevice>())).Returns(expectedUser).Verifiable();

        // Act
        
        var updateUser = _mockUserMessageRepo.Object.UpdateUserData(expectedUser);
        var getDevice = _mockSmartMeterRepo.Object.GetById(expectedUser.UserID);
        
        // Assert
        Assert.NotNull(updateUser);
        Assert.NotNull(getDevice);
        
        Assert.Equal(updateUser.SmartMeterId, getDevice.SmartMeterId);
        Assert.Equal(updateUser, getDevice.UserData);
        _saveData.Verify(saveData => saveData.ListToJson(It.IsAny<SmartDevice>()), Times.Once);
        _mockUserMessageRepo.Verify(repo => repo.UpdateUserData(expectedUser), Times.Once);
        _mockSmartMeterRepo.Verify(repo => repo.GetById(expectedUser.UserID), Times.Once);
    }
    
    [Theory]
    [InlineData(1001, "dummy11@example.com")]
    public void Repo_Should_Update_When_Does_Exist(int existingId, string email)
    {
        // Arrange
        var expectedData = new UserData
        {
            UserID = existingId,
            UserEmail = email
        };
    
        _mockUserMessageRepo.Setup(repo => repo.UpdateUserData(expectedData)).Returns(expectedData);
        _saveData.Setup(saveData => saveData.ListToJson(It.IsAny<SmartDevice>())).Returns(expectedData);
    
        // Act
        var result = _mockUserMessageRepo.Object.UpdateUserData(expectedData);
    
        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedData.SmartMeterId, result.SmartMeterId);
    }
    
    [Fact]
    public void UpdateUserRepo_WhenExceptionThrown_LogsErrorAndThrows()
    {
        // Arrange
        var userData = new UserData { UserID = 12345 };

        _saveData
            .Setup(data => data.ListToJson(It.IsAny<UserData>()))
            .Throws(new NullReferenceException("Object reference not set to an instance of an object."));

        // Act & Assert
        var exception = Assert.Throws<NullReferenceException>(() => _mockUserMessageRepo.Object.UpdateUserData(userData));
        Assert.Equal("Object reference not set to an instance of an object.", exception.Message);

        _mockErrorLogRepo.Verify(repo => repo.LogError(It.Is<ErrorLogMessage>(
            log => log.ClientId == userData.UserID &&
                   log.Message.Contains($"From UserID {userData.UserID} in UserMessageRepo:"))), Times.Once);
    }



 


}
