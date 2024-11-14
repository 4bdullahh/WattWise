using Moq;
using Newtonsoft.Json;
using Xunit;
using server_side.Repository.Interface;
using server_side.Repository.Models;
using server_side.Services;
using server_side.Services.Models;
using System;
using server_side.Services.Interface;

namespace server_side.Tests.Services
{
    public class SmartMeterServicesTests
    {
        private readonly Mock<IErrorLogRepo> _mockErrorLogRepo;
        private readonly Mock<ISmartMeterServices> _smartMeterServices;

        public SmartMeterServicesTests()
        {
            _mockErrorLogRepo = new Mock<IErrorLogRepo>();
            _smartMeterServices = new Mock<ISmartMeterServices>();
        }

        [Fact]
        public void UpdateMeterServices_WhenSmartMeterExists_ReturnsSmartMeterResponse()
        {
            // Arrange
            var smartDevice = new SmartMeterResponse
            {
                SmartMeterID = 12345,
                EnergyPerKwH = 100.0,
                CurrentMonthCost = 50.0,
                KwhUsed = 200.0,
                Message = "Current Cost is £50"
            };
            var decryptedMessage = JsonConvert.SerializeObject(smartDevice);
            _smartMeterServices.Setup(repo => repo.UpdateMeterServices(decryptedMessage)).Returns(smartDevice);
            // Act
            var response = _smartMeterServices.Object.UpdateMeterServices(decryptedMessage);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(smartDevice.SmartMeterID, response.SmartMeterID);
            Assert.Equal(smartDevice.EnergyPerKwH, response.EnergyPerKwH);
            Assert.Equal(smartDevice.CurrentMonthCost, response.CurrentMonthCost);
            Assert.Equal(smartDevice.KwhUsed, response.KwhUsed);
            Assert.Equal(smartDevice.Message, response.Message);
            
            _smartMeterServices.Verify(repo => repo.UpdateMeterServices(decryptedMessage), Times.Once);

        }

        [Fact]
        public void UpdateMeterServices_Should_Throw_NullReferenceException_When_Null_Returned()
        {
            // Arrange
            var smartDevice = new SmartDevice { SmartMeterId = 12345, };
            var decryptedMessage = JsonConvert.SerializeObject(smartDevice);
            
            var expectedResponse = new SmartMeterResponse { SmartMeterID = 12345 }; 
        
            _smartMeterServices.Setup(repo => repo.UpdateMeterServices(decryptedMessage)).Returns((SmartMeterResponse)null);
        
            // Act
            var response = _smartMeterServices.Object.UpdateMeterServices(decryptedMessage);
        
            // Assert
            Assert.Null(response);
            Assert.IsNotType<SmartMeterResponse>(response);
            Assert.NotEqual(expectedResponse, response);
            Assert.Throws<NullReferenceException>(() => {
                var smartMeterId = response.SmartMeterID;
            });
            _smartMeterServices.Verify(repo => repo.UpdateMeterServices(decryptedMessage), Times.Once);
        }
        
        [Fact]
        public void UpdateMeterServices_Should_Return_NotFoundMessage_When_SmartMeterDoesntExist()
        {
            // Arrange
            var smartDevice = new SmartDevice { SmartMeterId = 12345, };
            var decryptedMessage = JsonConvert.SerializeObject(smartDevice);
            
            var expectedResponse = new SmartMeterResponse
            {
                SmartMeterID = 12345,
                Message = "SmartMeter not found"
            }; 
        
            _smartMeterServices.Setup(repo => repo.UpdateMeterServices(decryptedMessage)).Returns(expectedResponse);
        
            // Act
            var response = _smartMeterServices.Object.UpdateMeterServices(decryptedMessage);
            
            // Assert
            Assert.NotNull(response);
            Assert.IsType<SmartMeterResponse>(response);
            Assert.Equal(expectedResponse, response);
       
            _smartMeterServices.Verify(repo => repo.UpdateMeterServices(decryptedMessage), Times.Once);
        }
        
        [Theory]
        [InlineData("Power grid outage")]
        [InlineData("Cost calculation")]
        public void UpdateMeterServices_Should_Contain_SubString_In_Message(string message)
        {
            // Arrange
            var smartDevice = new SmartDevice { SmartMeterId = 12345, };
            var decryptedMessage = JsonConvert.SerializeObject(smartDevice);
            
            var expectedResponse = new SmartMeterResponse
            {
                SmartMeterID = 12345,
                Message = message
            }; 
        
            _smartMeterServices.Setup(repo => repo.UpdateMeterServices(decryptedMessage)).Returns(expectedResponse);
        
            // Act
            var response = _smartMeterServices.Object.UpdateMeterServices(decryptedMessage);
            
            // Assert
            Assert.NotNull(response);
            Assert.IsType<SmartMeterResponse>(response);
            Assert.Contains(message, response.Message);
            Assert.Equal(expectedResponse, response);
            
            _smartMeterServices.Verify(repo => repo.UpdateMeterServices(decryptedMessage), Times.Once);
        }
        
        [Fact]
        public void UpdateMeterServices_WhenExceptionThrown_LogsErrorAndThrows()
        {
            // Arrange
            var smartDevice = new SmartDevice { SmartMeterId = 12345 };
            var decryptedMessage = JsonConvert.SerializeObject(smartDevice);
        
            _smartMeterServices.Setup(repo => repo.UpdateMeterServices(decryptedMessage))
                .Throws(new Exception("Database connection error"));
        
            // Act & Assert
            var exception = Assert.Throws<Exception>(() => _smartMeterServices.Object.UpdateMeterServices(decryptedMessage));
            Assert.Equal("Database connection error", exception.Message);
        
            _mockErrorLogRepo.Verify(repo => repo.LogError(It.Is<ErrorLogMessage>(
                log => log.ClientId == smartDevice.SmartMeterId &&
                       log.Message.Contains("Unable to access smart meter repo"))), Times.Once);
        }
    }
}
