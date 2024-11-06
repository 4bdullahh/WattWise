using Moq;
using Newtonsoft.Json;
using server_side.Repository;
using server_side.Services;
using server_side.Services.Interface;

public class SaveData_Tests
{
    private SaveData _saveData;
    private readonly Mock<IFolderPathServices> _mockFolderPathServices;

    public SaveData_Tests()
    {
        _mockFolderPathServices = new Mock<IFolderPathServices>();
        _saveData = new SaveData();
    }

    [Fact]
    public void ListToJson_UpdatesUser_WhenUserExistsInFile()
    {
        // Arrange
        var tempPath = Path.Combine(Path.GetTempPath(), "TestDataFolder", "server-side", "Data");
        Directory.CreateDirectory(tempPath);

        var userJsonPath = Path.Combine(tempPath, "UserJson.json");
      
        var initialUserJson = "[{\"UserID\": 1, \"firstName\": \"Jane\", \"lastName\": \"Smith\", \"UserEmail\": \"jane.smith@example.com\", \"Passcode\": \"oldpassword\", \"SmartDevice\": {\"SmartMeterID\": 1001, \"EnergyPerKwH\": 0.1, \"CurrentMonthCost\": 30.0}}]";
        File.WriteAllText(userJsonPath, initialUserJson);

        _mockFolderPathServices.Setup(x => x.GetWattWiseFolderPath()).Returns(Path.Combine(Path.GetTempPath(), "TestDataFolder"));
        
        var userData = new UserData
        {
            UserID = 1,
            firstName = "John",
            lastName = "Doe",
            Address = "123 Main St",
            UserEmail = "john.doe@example.com",
            Passcode = "password123",
            Hash = "hash",
            SmartMeterId = 1001,
            EnergyPerKwH = 0.2,
            CurrentMonthCost = 50.0
        };

        // Act
        var result = _saveData.ListToJson(userData);

        // Assert
        Assert.Equal(userData, result);

        // Clean up
        File.Delete(userJsonPath);
        Directory.Delete(tempPath, true);
    }

    [Fact]
    public void ListToJson_UpdatesDevice_WhenDeviceExistsInFile()
    {
        // Arrange
        var tempPath = Path.Combine(Path.GetTempPath(), "TestDataFolder", "server-side", "Data");
        Directory.CreateDirectory(tempPath);

        var meterJsonPath = Path.Combine(tempPath, "MeterJson.json");

        var initialMeterJson = "[{\"SmartMeterID\": 1001, \"EnergyPerKwH\": 0.1, \"CurrentMonthCost\": 40.0, \"KwdUsed\": 150, \"CustomerType\": \"Commercial\"}]";
        File.WriteAllText(meterJsonPath, initialMeterJson);

        _mockFolderPathServices.Setup(x => x.GetWattWiseFolderPath()).Returns(Path.Combine(Path.GetTempPath(), "TestDataFolder"));

        var smartDevice = new SmartDevice
        {
            SmartMeterId = 1001,
            EnergyPerKwH = 0.3,
            CurrentMonthCost = 60.0,
            KwhUsed = 200,
            CustomerType = "Residential"
        };

        // Act
        var result = _saveData.ListToJson(smartDevice);

        // Assert
        Assert.Equal(smartDevice, result);

        // Clean up
        File.Delete(meterJsonPath);
        Directory.Delete(tempPath, true);
    }
    [Fact]
    public void ListToJson_AddsNewUser_WhenUserDoesNotExistInFile()
    {
        // Arrange
        var tempPath = Path.Combine(Path.GetTempPath(), "TestDataFolder", "server-side", "Data");
        Directory.CreateDirectory(tempPath);

        var userJsonPath = Path.Combine(tempPath, "UserJson.json");
        File.WriteAllText(userJsonPath, "[]");  // Empty user list

        _mockFolderPathServices.Setup(x => x.GetWattWiseFolderPath()).Returns(Path.Combine(Path.GetTempPath(), "TestDataFolder"));

        var userData = new UserData
        {
            UserID = 2,
            firstName = "John",
            lastName = "Doe",
            Address = "123 Main St",
            UserEmail = "john.doe@example.com",
            Passcode = "password123",
            Hash = "hash",
            SmartMeterId = 1002,
            EnergyPerKwH = 0.2,
            CurrentMonthCost = 50.0
        };

        // Act
        var result = _saveData.ListToJson(userData);

        // Assert
        Assert.Equal(userData, result);

        // Clean up
        File.Delete(userJsonPath);
        Directory.Delete(tempPath, true);
    }
    [Fact]
    public void ListToJson_AddsNewDevice_WhenDeviceDoesNotExistInFile()
    {
        // Arrange
        var tempPath = Path.Combine(Path.GetTempPath(), "TestDataFolder", "server-side", "Data");
        Directory.CreateDirectory(tempPath);

        var meterJsonPath = Path.Combine(tempPath, "MeterJson.json");
        File.WriteAllText(meterJsonPath, "[]");  // Empty device list

        _mockFolderPathServices.Setup(x => x.GetWattWiseFolderPath()).Returns(Path.Combine(Path.GetTempPath(), "TestDataFolder"));

        var smartDevice = new SmartDevice
        {
            SmartMeterId = 1002,
            EnergyPerKwH = 0.3,
            CurrentMonthCost = 60.0,
            KwhUsed = 200,
            CustomerType = "Residential"
        };

        // Act
        var result = _saveData.ListToJson(smartDevice);

        // Assert
        Assert.Equal(smartDevice, result);

        // Clean up
        File.Delete(meterJsonPath);
        Directory.Delete(tempPath, true);
    }
    [Fact]
    public void ListToJson_ThrowsException_WhenUnsupportedDataType()
    {
        // Arrange
        var unsupportedData = new { Name = "Test" };

        // Act
        Action act = () => _saveData.ListToJson(unsupportedData);

        // Assert
        Assert.Throws<ArgumentException>(act);
    }
}
