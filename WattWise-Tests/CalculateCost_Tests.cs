using Moq;
using server_side.Repository;

namespace WattWise_Tests;

public class CalculateCost_Tests
{

    [Theory]
    [InlineData(
        0.21,
        0.60,
        0.21
    )]
    //Below used to fail test
    // [InlineData(
    //     1,
    //     11,
    //     5
    // )]
    public void Should_Calculate_Day_Cost_And_Return_Double(
            double kwhPerHour, 
            double standingCharge, 
            double costPerKwh
        )
    {
        //Arrange
        var dateTime = DateTime.Now;
        double currentDayCost = costPerKwh * (kwhPerHour * dateTime.Hour);
        double daysPassedInMonth = dateTime.Day;
        double currentMonthCost = currentDayCost * daysPassedInMonth;
        double calcStandingCharge = standingCharge * daysPassedInMonth;
        double totalCost = currentMonthCost + calcStandingCharge;
        double expectedCost = Math.Round(totalCost, 2);
        
        //Act
        CalculateCost calculateCost = new CalculateCost();
        var actualCost = calculateCost.CalculateRates(dateTime, kwhPerHour, standingCharge, costPerKwh);
        
        //Assert
        Assert.Equal(expectedCost, actualCost, 2);
        
    }
}