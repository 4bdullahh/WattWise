using System;
using Moq;
using Xunit;
using server_side.Repository;
using server_side.Repository.Interface;

public class CalculateCostTests
{
    private readonly CalculateCost _calculateCost;
    private readonly Mock<IErrorLogRepo> _errorLog;
    public CalculateCostTests()
    {
        _errorLog = new Mock<IErrorLogRepo>();
        _calculateCost = new CalculateCost(_errorLog.Object);
    }

    [Theory]
    [InlineData("Large Household", 18.0)]
    [InlineData("Average Household", 12.0)]
    [InlineData("Small Household", 8.0)]
    [InlineData("Unknown", 0.0)]
    public void GetCurrentBill_ShouldCalculateCorrectKwhBasedOnCustomerType(string customerType, double expectedDailyUsage)
    {
        // Arrange
        var calculateCost = new CalculateCost(Mock.Of<IErrorLogRepo>()); // Mock dependency
        var device = new SmartDevice { CustomerType = customerType };

        DateTime now = DateTime.Now;
        DateTime billingStartDate = new DateTime(now.Year, now.Month, 1);
        double minutesInMonth = (now - billingStartDate).TotalMinutes;
        double expectedKwhUsed = (expectedDailyUsage / (24 * 60)) * minutesInMonth;

        // Act
        var updatedDevice = calculateCost.getCurrentBill(device);

        // Assert
        Assert.Equal(Math.Round(expectedKwhUsed, 2), Math.Round(updatedDevice.KwhUsed, 2));
    }


    [Fact]
    public void GetCurrentBill_ShouldReturnCorrectMonthlyCost()
    {
        // Arrange
        var device = new SmartDevice { CustomerType = "Average Household" };

        // Act
        var updatedDevice = _calculateCost.getCurrentBill(device);

        double expectedEnergyCost = updatedDevice.KwhUsed * updatedDevice.CostPerKwh;
        int daysInMonth = DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month);
        double expectedTotalCost = expectedEnergyCost + (updatedDevice.StandingCharge * daysInMonth);

        // Assert
        Assert.Equal(Math.Round(expectedTotalCost, 2), Math.Round(updatedDevice.CurrentMonthCost, 2));
    }

    [Fact]
    public void GetCurrentBill_ShouldSetRatesCorrectly()
    {
        // Arrange
        var device = new SmartDevice { CustomerType = "Small Household" };

        // Act
        var updatedDevice = _calculateCost.getCurrentBill(device);

        // Assert 
        Assert.Equal(0.60, updatedDevice.StandingCharge);
        Assert.Equal(0.24, updatedDevice.CostPerKwh);
        Assert.True(updatedDevice.KwhUsed > 0);
        Assert.True(updatedDevice.CurrentMonthCost > 0);
    }
    [Fact]
    public void GetCurrentBill_ShouldHandleNullSmartDeviceGracefully()
    {
        // Arrange
        SmartDevice device = null;

        // Act
        var result = _calculateCost.getCurrentBill(device);

        // Assert
        Assert.NotNull(result); 
        Assert.Equal(0, result.KwhUsed);
        Assert.Equal(0, result.CurrentMonthCost);
    }


    [Fact]
    public void GetCurrentBill_ShouldHandleEmptyCustomerType()
    {
        // Arrange
        var device = new SmartDevice { CustomerType = "" };

        // Act
        var updatedDevice = _calculateCost.getCurrentBill(device);

        // Assert
        Assert.Equal(0, updatedDevice.KwhUsed); 
        Assert.Equal(0, updatedDevice.CurrentMonthCost); 
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-10)]
    public void GetCurrentBill_ShouldHandleNegativeKwhUsageGracefully(double invalidKwh)
    {
        // Arrange
        var device = new SmartDevice { CustomerType = "Average Household", KwhUsed = invalidKwh };

        // Act
        var updatedDevice = _calculateCost.getCurrentBill(device);

        // Assert
        Assert.True(updatedDevice.KwhUsed >= 0); 
    }

    [Fact]
    public void GetCurrentBill_ShouldHandleExtremeHighUsage()
    {
        // Arrange
        var device = new SmartDevice { CustomerType = "Large Household" };

        // Act
        var updatedDevice = _calculateCost.getCurrentBill(device);

        updatedDevice.KwhUsed = double.MaxValue;
        updatedDevice.CurrentMonthCost = _calculateCost.CalculateRates(updatedDevice.KwhUsed, updatedDevice.StandingCharge, updatedDevice.CostPerKwh, 15.0);

        // Assert
        Assert.NotEqual(double.PositiveInfinity, updatedDevice.CurrentMonthCost); // Ensure no overflow occurs
    }
}
