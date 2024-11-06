using Moq;
using server_side.Repository.Interface;

namespace WattWise_Tests;

public class CostService_Tests
{
     private readonly Mock<ICalculateCost> _calculateCostMock;
    private readonly Mock<ISmartMeterRepo> _smartMeterRepoMock;
    private readonly CostUpdateService _service;

    public CostService_Tests()
    {
        _calculateCostMock = new Mock<ICalculateCost>();
        _smartMeterRepoMock = new Mock<ISmartMeterRepo>();
        _service = new CostUpdateService(_calculateCostMock.Object, _smartMeterRepoMock.Object);
    }

    [Fact]
    public async Task StartAsync_ShouldHandleNullSmartDevicesListGracefully()
    {
        // Arrange
        _smartMeterRepoMock.Setup(repo => repo.LoadSmartMeterData()).Returns((List<SmartDevice>)null);

        // Act
        await _service.StartAsync(CancellationToken.None);
        _service.UpdateCosts(null);

        // Assert
        _calculateCostMock.Verify(service => service.getCurrentBill(It.IsAny<SmartDevice>()), Times.Never);
    }

    [Fact]
    public void UpdateCosts_ShouldHandleEmptySmartDevicesList()
    {
        // Arrange
        _smartMeterRepoMock.Setup(repo => repo.LoadSmartMeterData()).Returns(new List<SmartDevice>());

        // Act
        _service.UpdateCosts(null);

        // Assert
        _calculateCostMock.Verify(service => service.getCurrentBill(It.IsAny<SmartDevice>()), Times.Never);
    }

    [Fact]
    public void UpdateCosts_ShouldHandleExceptionDuringCostCalculation()
    {
        // Arrange
        var smartDevices = new List<SmartDevice> { new SmartDevice() };
        _smartMeterRepoMock.Setup(repo => repo.LoadSmartMeterData()).Returns(smartDevices);

        _calculateCostMock.Setup(service => service.getCurrentBill(It.IsAny<SmartDevice>())).Throws(new Exception("Calculation Error"));

        // Act
        _service.UpdateCosts(null); 

        // Assert
        _calculateCostMock.Verify(service => service.getCurrentBill(It.IsAny<SmartDevice>()), Times.Once);
    }

    [Fact]
    public async Task StopAsync_ShouldHandleMultipleStopsGracefully()
    {
        // Act
        await _service.StartAsync(CancellationToken.None);
        await _service.StopAsync(CancellationToken.None); 
        await _service.StopAsync(CancellationToken.None); 

        // Assert
        _calculateCostMock.Verify(service => service.getCurrentBill(It.IsAny<SmartDevice>()), Times.Never);
    }
    
    [Fact]
    public void UpdateCosts_ShouldNotCallGetCurrentBillIfSmartDeviceListIsEmpty()
    {
        // Arrange
        _smartMeterRepoMock.Setup(repo => repo.LoadSmartMeterData()).Returns(new List<SmartDevice>());

        // Act
        _service.UpdateCosts(null);

        // Assert
        _calculateCostMock.Verify(service => service.getCurrentBill(It.IsAny<SmartDevice>()), Times.Never);
    }
    
    [Fact]
    public void UpdateCosts_ShouldHandleInvalidSmartDeviceDataGracefully()
    {
        // Arrange
        var smartDevices = new List<SmartDevice> { new SmartDevice { CustomerType = null } };
        _smartMeterRepoMock.Setup(repo => repo.LoadSmartMeterData()).Returns(smartDevices);

        // Act
        _service.UpdateCosts(null);

        // Assert
        _calculateCostMock.Verify(service => service.getCurrentBill(It.IsAny<SmartDevice>()), Times.Once);
    }
}