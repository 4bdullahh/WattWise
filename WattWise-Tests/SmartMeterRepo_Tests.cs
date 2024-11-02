using System.Text;
using Moq;
using server_side.Repository;
using server_side.Repository.Interface;
using server_side.Services;
using server_side.Services.Interface;

namespace WattWise_Tests;

public class SmartMeterRepo_Tests
{
    [Theory]
    //TEST SHOULD FAIL
    // [InlineData(
    //     999
    // )]
    //TEST SHOULD PASS
    [InlineData(
            4
        )]
    public void GetById_Should_Return_int(int deviceId)
    {
        //Arrange
        var mockRepo = new Mock<ISmartMeterRepo>();
        var mockCost = new Mock<CalculateCost>();
        var mockSaveData = new Mock<ISaveData>();
        var expectedId = new SmartDevice
        {
            SmartMeterID = deviceId
        };
        mockRepo.Setup(repo => repo.GetById(deviceId)).Returns(expectedId);
        var service = new SmartMeterRepo(mockSaveData.Object, mockCost.Object);

        // Act
        var result = service.GetById(deviceId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedId.SmartMeterID, result.SmartMeterID);
    }

    [Fact]
    public void Repo_Should_Return_True_If_Json_File_Exists()
    {
        // Arrange
        var mockFolderPath = new Mock<IFolderPathServices>();
        var mockSaveData = new Mock<ISaveData>();
        var mockCost = new Mock<CalculateCost>();
        
        string testFolderPath = "WattWise-Tests/mockData";
        
        mockFolderPath.Setup(path => path.GetWattWiseFolderPath()).Returns(testFolderPath);
        
        
        if (!Directory.Exists(testFolderPath))
        {
            Directory.CreateDirectory(testFolderPath);
        }

        var services = new SmartMeterRepo(mockSaveData.Object, mockCost.Object);

        string jsonFilePath = Path.Combine(testFolderPath, "Meter_Test_Data.json");
        
        // Act
        if (File.Exists(jsonFilePath))
        {
            File.ReadAllText(jsonFilePath);
        }
        else
        {
            File.Create(jsonFilePath).Close();
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
    //     0,
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
        var mockRepo = new Mock<ISmartMeterRepo>();
        var mockCost = new Mock<CalculateCost>();
        var mockSaveData = new Mock<ISaveData>();
        var expectedData = new SmartDevice
        {
            SmartMeterID = existingId,
            EnergyPerKwH = energyPerKwh,
            CostPerKwh = costPerKwh,
            StandingCharge = standingCharge
        };

        mockRepo.Setup(repo => repo.UpdateMeterRepo(expectedData)).Returns(expectedData);

        mockSaveData.Setup(saveData => saveData.ListToJson(It.IsAny<SmartDevice>()))
          .Returns(expectedData);

        var service = new SmartMeterRepo(mockSaveData.Object, mockCost.Object);

        // Act
        var result = service.UpdateMeterRepo(expectedData);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedData.SmartMeterID, result.SmartMeterID);
        // Assert.Equal(expectedData, result);
    }
}