using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using server_side.Repository;
using server_side.Repository.Interface;
using server_side.Repository.Models;
using server_side.Services.Interface;

namespace WattWise_Tests;

public class PowerGrid_Tests
{
    private readonly Mock<ISaveData> _saveData;
    private readonly Mock<IFolderPathServices> _folderPath;
    private readonly Mock<ISmartMeterRepo> _mockSmartMeterRepo;
    private readonly Mock<IErrorLogRepo>  _mockErrorLogRepo;
    private List<SmartDevice> _clientList;
    private readonly Mock<IPowerGridCalc> _mockPowerGridCalc;
    private PowerGridTracker _powerGridTracker;

    
    public PowerGrid_Tests()
    {
        _mockErrorLogRepo = new Mock<IErrorLogRepo>();
        _saveData = new Mock<ISaveData>();
        _folderPath = new Mock<IFolderPathServices>();
        _mockSmartMeterRepo = new Mock<ISmartMeterRepo>();
        _mockErrorLogRepo = new Mock<IErrorLogRepo>();
        _mockPowerGridCalc = new Mock<IPowerGridCalc>();
        _powerGridTracker = new PowerGridTracker();
    }

    [Fact]
    public void CalculateGridOutage_Should_Return_Null_When_KwhLimit_Not_Reached()
    {
        // Arrange
        var smartDevice = new SmartDevice { SmartMeterId = 0, EnergyPerKwH = 0.001 };
         _mockPowerGridCalc.Setup(pc => pc.CalculateGridOutage(smartDevice)).Returns((SmartDevice)null);
         
        // Act
        var result = _mockPowerGridCalc.Object.CalculateGridOutage(smartDevice);

        // Assert
        Assert.Null(result);
    }


    [Theory]
    [InlineData(0,1.1)]
    [InlineData(1,2.4)]
    [InlineData(3,0.6)]
    public void PowerGrid_Should_Reset_KwhLimit_When_Reached(int smartDeviceId, double energyPerKwH)
    {
        // Arrange
        var expectedDevice = new SmartDevice
        {
            SmartMeterId = smartDeviceId,
            EnergyPerKwH = energyPerKwH
        };
        
        _mockPowerGridCalc.Setup(pc => pc.CalculateGridOutage(expectedDevice)).Returns(expectedDevice);
        
        // Act
        var result = _mockPowerGridCalc.Object.CalculateGridOutage(expectedDevice);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedDevice.SmartMeterId, result.SmartMeterId);
        
    }
    

}