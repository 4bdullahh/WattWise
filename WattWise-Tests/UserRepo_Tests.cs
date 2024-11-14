﻿using FizzWare.NBuilder.Extensions;
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
    private readonly Mock<IUserMessageRepo> _mockUserMessageRepo;
    private readonly Mock<ISmartMeterRepo> _mockSmartMeterRepo;
    private readonly Mock<IErrorLogRepo>  _mockErrorLogRepo;

    public UserRepo_Tests()
    {
        _mockErrorLogRepo = new Mock<IErrorLogRepo>();
        _saveData = new Mock<ISaveData>();
        _folderPathServices = new Mock<IFolderPathServices>();
        _mockUserMessageRepo = new Mock<IUserMessageRepo>();
        _mockErrorLogRepo = new Mock<IErrorLogRepo>();
        _mockSmartMeterRepo = new Mock<ISmartMeterRepo>();
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
    
    [Fact]
    public void GetById_Should_Throw_Exception_If_ID_Returns_Null()
    {
        //Arrange
        var expectedResult = new UserData
        {
            UserID = 10,
        };
        _mockUserMessageRepo.Setup(repo => repo.GetById(expectedResult.UserID)).Returns((UserData)null);
    
        // Act
        var actualResult = _mockUserMessageRepo.Object.GetById(expectedResult.UserID);
    
        // Assert
        Assert.Null(actualResult);
        Assert.NotEqual(expectedResult, actualResult);
        
        Assert.Throws<NullReferenceException>(() => {
            var smartMeterId = actualResult.SmartMeterId;
        });
        _mockUserMessageRepo.Verify(repo => repo.GetById(expectedResult.UserID), Times.Once);
    }
    
    [Theory]
    [InlineData(4)]
    public void Repo_Should_Return_Same_SmartMeterID_For_UserData(int deviceId)
    {
        // Arrange
        var expectedUser = new UserData
        {
            UserID = 1001,
            UserEmail = "dummy1@example.com",
            CostPerKwh = 0.24,
            SmartMeterId = deviceId
        };
        
        var smartDevice = new SmartDevice
        {
            SmartMeterId = deviceId
        };
      
        _mockUserMessageRepo.Setup(repo => repo.UpdateUserData(expectedUser)).Returns(expectedUser).Verifiable();
        _mockSmartMeterRepo.Setup(meter => meter.GetById(smartDevice.SmartMeterId)).Returns(smartDevice).Verifiable();

        // Act
        var updateUser = _mockUserMessageRepo.Object.UpdateUserData(expectedUser);
        var getDevice = _mockSmartMeterRepo.Object.GetById(expectedUser.UserID);
        
        // Assert
        Assert.NotNull(updateUser);
        Assert.NotNull(getDevice);
        
        Assert.Equal(updateUser.SmartMeterId, getDevice.SmartMeterId);
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
    public void GetById_Check_If_Returned_Data_Mismatch()
    {
        //Arrange
        var expectedResult = new UserData
        {
            UserID = 10,
        };

        var smartDevice = new SmartDevice
        {
            SmartMeterId = 10
        };
        _mockUserMessageRepo.Setup(repo => repo.GetById(expectedResult.UserID)).Returns(smartDevice);
    
        // Act
        var actualResult = _mockUserMessageRepo.Object.GetById(expectedResult.UserID);
    
        // Assert
        Assert.NotNull(actualResult);
        Assert.NotEqual(expectedResult, actualResult);
        Assert.IsType<SmartDevice>(actualResult); 
        
        _mockUserMessageRepo.Verify(repo => repo.GetById(expectedResult.SmartMeterId), Times.Once);
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
