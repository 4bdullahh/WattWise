using System.Text;
using Moq;
using server_side.Repository;
using server_side.Repository.Interface;
using server_side.Repository.Models;
using server_side.Services.Interface;
using Xunit.Sdk;

namespace WattWise_Tests;

public class SmartMeterRepo_Tests
{
    
    private readonly Mock<ISaveData> _saveData;
    private readonly Mock<IFolderPathServices> _folderPathServices;
    private readonly Mock<ISmartMeterRepo> _mockSmartMeterRepo;
    private readonly Mock<IUserMessageRepo> _mockUserMessageRepo;
    private readonly Mock<IErrorLogRepo>  _mockErrorLogRepo;
    private readonly Mock<ICalculateCost> _mockCalculateCost;

    public SmartMeterRepo_Tests()
    {
        _mockErrorLogRepo = new Mock<IErrorLogRepo>();
        _saveData = new Mock<ISaveData>();
        _folderPathServices = new Mock<IFolderPathServices>();
        _mockSmartMeterRepo = new Mock<ISmartMeterRepo>();
        _mockUserMessageRepo = new Mock<IUserMessageRepo>();
        _mockErrorLogRepo = new Mock<IErrorLogRepo>();
        _mockCalculateCost = new Mock<ICalculateCost>();
    }
    
    [Theory]
    //Test should pass
    [InlineData(
            4
        )]
    
    //Test should fail
    // [InlineData(
    //     999
    // )]
    public void GetById_Should_Return_int(int deviceId)
    {
        //Arrange
        var expectedId = new SmartDevice
        {
            SmartMeterId = deviceId,
        };
        _mockSmartMeterRepo.Setup(repo => repo.GetById(deviceId)).Returns(expectedId);

        // Act
        var result = _mockSmartMeterRepo.Object.GetById(deviceId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedId.SmartMeterId, result.SmartMeterId);
    }
    
    [Fact]
    public void GetById_Should_Throw_Exception_If_ID_Returns_Null()
    {
        //Arrange
        var expectedResult = new SmartDevice
        {
            SmartMeterId = 10,
        };
        _mockSmartMeterRepo.Setup(repo => repo.GetById(expectedResult.SmartMeterId)).Returns((SmartDevice)null);
    
        // Act
        var actualResult = _mockSmartMeterRepo.Object.GetById(expectedResult.SmartMeterId);
    
        // Assert
        Assert.Null(actualResult);
        Assert.NotEqual(expectedResult, actualResult);
        
        Assert.Throws<NullReferenceException>(() => {
            var smartMeterId = actualResult.SmartMeterId;
        });
        _mockSmartMeterRepo.Verify(repo => repo.GetById(expectedResult.SmartMeterId), Times.Once);
    }
    
    [Fact]
    public void GetById_Should_Throw_Exception_If_Data_Mismatch()
    {
        //Arrange
        var expectedResult = new SmartDevice
        {
            SmartMeterId = 10,
        };

        var userData = new UserData
        {
            UserID = 10
        };
        _mockSmartMeterRepo.Setup(repo => repo.GetById(expectedResult.SmartMeterId)).Returns(userData);
    
        // Act
        var actualResult = _mockSmartMeterRepo.Object.GetById(expectedResult.SmartMeterId);
    
        // Assert
        Assert.NotNull(actualResult);
        Assert.NotEqual(expectedResult, actualResult);
        
        Assert.Throws<InvalidCastException>(() => {
            var smartDeviceResult = actualResult;
            var smartMeterId = smartDeviceResult.SmartMeterId; 
        });
        _mockSmartMeterRepo.Verify(repo => repo.GetById(expectedResult.SmartMeterId), Times.Once);
    }
    
    [Fact]
    public void Repo_Should_Return_True_If_Json_File_Exists()
    {
        // Arrange
        var mockFolderPath = new Mock<IFolderPathServices>();
        
        string testFolderPath = "WattWise-Tests/mockData";
        
        mockFolderPath.Setup(path => path.GetWattWiseFolderPath()).Returns(testFolderPath);
        
        
        if (!Directory.Exists(testFolderPath))
        {
            Directory.CreateDirectory(testFolderPath);
        }

        _mockSmartMeterRepo.Object.LoadSmartMeterData();

        string jsonFilePath = Path.Combine(testFolderPath, "Meter_Test_Data.json");
        
        // Act
        if (File.Exists(jsonFilePath))
        {
            File.ReadAllText(jsonFilePath);
        }
  
        // Assert
        Assert.NotNull(jsonFilePath);
    }


    [Theory]
    //Test Should Fail
    // [InlineData(
    //     999,
    //     0,
    //     0,
    //     0
    // )]
    //
    //Test Should Pass
    [InlineData(
        4,
        0.21,
        0.60,
        0.24
        )]
    public void Repo_Should_Update_When_Does_Exist(int existingId, double energyPerKwh, double standingCharge, double costPerKwh)
    {
        //Arrange
        var expectedData = new SmartDevice
        {
            SmartMeterId = existingId,
            EnergyPerKwH = energyPerKwh,
            CostPerKwh = costPerKwh,
            StandingCharge = standingCharge
        };

        _mockSmartMeterRepo.Setup(repo => repo.UpdateMeterData(expectedData)).Returns(expectedData);
        
        // Act
        var result = _mockSmartMeterRepo.Object.UpdateMeterData(expectedData);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedData.SmartMeterId, result.SmartMeterId);
        _mockSmartMeterRepo.Verify(repo => repo.UpdateMeterData(expectedData), Times.Once);
    }
    
    [Fact]
    public void Repo_Should_ThrowException_When_Data_Mismatch()
    {
        //Arrange
        var expectedData = new SmartDevice
        {
            SmartMeterId = 2,
            EnergyPerKwH = 0.21,
            CostPerKwh = 0.24,
            StandingCharge = 0.60
        };

        var userData = new UserData
        {
            UserID = 1
        };

        _mockSmartMeterRepo.Setup(repo => repo.UpdateMeterData(expectedData)).Returns(userData);
        
        //Act
        var result = _mockSmartMeterRepo.Object.UpdateMeterData(expectedData);
        //Assert
        Assert.NotNull(result);
        Assert.NotEqual(expectedData, result);
        _mockSmartMeterRepo.Verify(repo => repo.UpdateMeterData(expectedData), Times.Once);
    }
}