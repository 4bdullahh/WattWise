using Castle.Core.Logging;
using Moq;
using server_side.Repository;
using server_side.Repository.Interface;

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
        mockRepo.Verify(repo => repo.GetById(deviceId), Times.Once);
       
    }
    
    
     [Theory]
    //Test Should Pass
     // [InlineData(
     //     999,
     //     0.21
     // )]
     //
    //Test Should Pass
     [InlineData(
         4,
         0.21,
         0.60,
         0.24
         )]
     public void UpdateMeterRepo_Should_Update_When_Does_Exist(int existingId, double energyPerKwh, double standingCharge, double costPerKwh)
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
         var service = new SmartMeterRepo(mockSaveData.Object, mockCost.Object);

         // Act
         var result = service.UpdateMeterRepo(expectedData);

         // Assert
         Assert.NotNull(result);
         Assert.Equal(expectedData.SmartMeterID, result.SmartMeterID);
         mockRepo.Verify(repo => repo.UpdateMeterRepo(expectedData), Times.Once);
         
     }
    
}