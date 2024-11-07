using Moq;
using server_side.Repository;
using server_side.Repository.Interface;
using server_side.Services.Interface;

public class SmartMeterRepo_Tests
{
    private readonly Mock<IErrorLogRepo> _errorLogRepo;
    private readonly Mock<ISaveData> _saveData;
    private readonly Mock<ICalculateCost> _calculateCost;
    private readonly Mock<IFolderPathServices> _folderPathServices;
    private readonly Mock<ISmartMeterRepo> _mockRepo;
    private SmartMeterRepo _service;

    public SmartMeterRepo_Tests()
    {
        _errorLogRepo = new Mock<IErrorLogRepo>();
        _saveData = new Mock<ISaveData>();
        _calculateCost = new Mock<ICalculateCost>();
        _folderPathServices = new Mock<IFolderPathServices>();
        _mockRepo = new Mock<ISmartMeterRepo>();

        _service = new SmartMeterRepo(_saveData.Object, _calculateCost.Object, _errorLogRepo.Object);
    }

    [Theory]
    [InlineData(4)]
    public void GetById_Should_Return_int(int deviceId)
    {
        // Arrange
        var expectedDevice = new SmartDevice { SmartMeterId = deviceId };
        _mockRepo.Setup(repo => repo.GetById(deviceId)).Returns(expectedDevice);

        // Act
        var result = _mockRepo.Object.GetById(deviceId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedDevice.SmartMeterId, result.SmartMeterId);
    }

    [Fact]
    public void Repo_Should_Return_True_If_Json_File_Exists()
    {
        string testFolderPath = "WattWise-Tests/mockData";
        _folderPathServices.Setup(path => path.GetWattWiseFolderPath()).Returns(testFolderPath);
        
        if (!Directory.Exists(testFolderPath))
        {
            Directory.CreateDirectory(testFolderPath);
        }
    
        string jsonFilePath = Path.Combine(testFolderPath, "Meter_Test_Data.json");
    
        Assert.NotNull(jsonFilePath);
        Assert.True(File.Exists(jsonFilePath));
    }

    [Theory]
    [InlineData(4, 0.21, 0.60, 0.24)]
    public void Repo_Should_Update_When_Does_Exist(int existingId, double energyPerKwh, double standingCharge, double costPerKwh)
    {
        // Arrange
        var expectedData = new SmartDevice
        {
            SmartMeterId = existingId,
            EnergyPerKwH = energyPerKwh,
            CostPerKwh = costPerKwh,
            StandingCharge = standingCharge
        };

        _mockRepo.Setup(repo => repo.UpdateMeterData(expectedData)).Returns(expectedData);
        _saveData.Setup(saveData => saveData.ListToJson(It.IsAny<SmartDevice>())).Returns(expectedData);

        // Act
        var result = _mockRepo.Object.UpdateMeterData(expectedData);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedData.SmartMeterId, result.SmartMeterId);
    }
}
