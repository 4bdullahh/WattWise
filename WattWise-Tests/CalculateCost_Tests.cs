/*using Moq;
using server_side.Repository;

namespace WattWise_Tests;

public class CalculateCost_Tests
{
    [Theory]
    [InlineData(
        0.21,
        0.60,
        0.24
    )]
    //Below used to fail test
    // [InlineData(
    //     1,
    //     11,
    //     5
    // )]
    public void Should_Calculate_Day_Cost_And_Return_Double_With_Two_Decimal_Points(
            double kwhPerHour, 
            double standingCharge, 
            double costPerKwh
        )
    {
        //Arrange
        var dateTime = DateTime.Now;
        var currentDayCost = costPerKwh * (kwhPerHour * dateTime.Hour);
        var daysPassedInMonth = dateTime.Day;
        var currentMonthCost = currentDayCost * daysPassedInMonth;
        var calcStandingCharge = standingCharge * daysPassedInMonth;
        var totalCost = currentMonthCost + calcStandingCharge;
        var expectedCost = Math.Round(totalCost, 2);
       
        //Act
        CalculateCost calculateCost = new CalculateCost();
        var actualCost = calculateCost.CalculateRates(dateTime, kwhPerHour, standingCharge, costPerKwh);
        
        //Assert
        Assert.NotNull(actualCost);
        Assert.True(actualCost >= 0, "Expected cost to be non negative");
        Assert.Equal(expectedCost, actualCost);
    }
    

    [Fact]
    public void Get_Current_Bill_Should_Return_Model()
    {
        //Arrange
        var smartDevice = new SmartDevice();
        //Act
        var getCurrentBill = new CalculateCost();
        var actualResult = getCurrentBill.getCurrentBill(smartDevice);
        //Assert
        Assert.NotNull(actualResult);
        Assert.Equal(smartDevice, actualResult);
    }
    
}*/