using Moq;
using server_side.Repository;
using server_side.Repository.Interface;

namespace WattWise_Tests;

public class SmartMeterRepo_Tests
{
    private readonly SmartMeterRepo _repo;
    

    [Theory]
    
    // Test Should Fail
    [InlineData(
            4
        )]
    public void GetById_Should_Return_int(int meterId)
    {
        //Arrange
        var expectedId = meterId;
        
        //Act
        var returnedId = _repo.GetById(expectedId);
        var actualResult = returnedId.SmartMeterID;
       
        //Assert
        Assert.NotNull(actualResult);
        Assert.Equal(expectedId, actualResult);
    }
    
    
    [Theory]
    //Test Should Pass
    // [InlineData(
    //     999,
    //     0.21
    // )]
    
    //Test Should Fail
    [InlineData(
        4,
        0.21,
        0.60,
        0.24
        )]
    public void UpdateMeterRepo_Should_Update_When_Does_Exist(int existingId, double energyPerKwh, double standingCharge, double costPerKwh)
    {
        // Arrange
        var curTime = DateTime.Now;
        var currentCost = new CalculateCost();
        var returnedTotalCost = currentCost.CalculateRates(curTime, energyPerKwh, standingCharge, costPerKwh);
        
        var smartDevice = new SmartDevice 
            {   
                SmartMeterID = existingId, 
                EnergyPerKwH = energyPerKwh,
                CurrentMonthCost = returnedTotalCost,
            }; 
        
        //Act
        var result = _repo.UpdateMeterRepo(smartDevice);
         
        // Assert
        Assert.NotNull(result);
        Assert.Equal(smartDevice.SmartMeterID, result.SmartMeterID);
        
    }
}