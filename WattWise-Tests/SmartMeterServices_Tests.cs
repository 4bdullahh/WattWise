using Moq;
using Newtonsoft.Json;
using Xunit;
using server_side.Repository.Interface;
using server_side.Repository.Models;
using server_side.Services;
using server_side.Services.Models;
using System;

namespace server_side.Tests.Services
{
    public class SmartMeterServicesTests
    {
        private readonly Mock<ISmartMeterRepo> _mockSmartMeterRepo;
        private readonly Mock<IErrorLogRepo> _mockErrorLogRepo;
        private readonly SmartMeterServices _smartMeterServices;

        public SmartMeterServicesTests()
        {
            _mockSmartMeterRepo = new Mock<ISmartMeterRepo>();
            _mockErrorLogRepo = new Mock<IErrorLogRepo>();
            _smartMeterServices = new SmartMeterServices(_mockSmartMeterRepo.Object, _mockErrorLogRepo.Object);
        }

        [Fact]
        public void UpdateMeterServices_WhenSmartMeterExists_ReturnsSmartMeterResponse()
        {
            // Arrange
            var smartDevice = new SmartDevice
            {
                SmartMeterId = 12345,
                EnergyPerKwH = 100.0,
                CurrentMonthCost = 50.0,
                KwhUsed = 200.0
            };
            var decryptedMessage = JsonConvert.SerializeObject(smartDevice);

            _mockSmartMeterRepo
                .Setup(repo => repo.UpdateMeterData(It.IsAny<SmartDevice>()))
                .Returns(smartDevice);

            // Act
            var response = _smartMeterServices.UpdateMeterServices(decryptedMessage);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(smartDevice.SmartMeterId, response.SmartMeterID);
            Assert.Equal(smartDevice.EnergyPerKwH, response.EnergyPerKwH);
            Assert.Equal(smartDevice.CurrentMonthCost, response.CurrentMonthCost);
            Assert.Equal(smartDevice.KwhUsed, response.KwhUsed);
            Assert.Equal($"Current Month Cost {smartDevice.CurrentMonthCost}", response.Message);
        }

        [Fact]
        public void UpdateMeterServices_WhenSmartMeterDoesNotExist_ReturnsNotFoundResponse()
        {
            // Arrange
            var smartDevice = new SmartDevice { SmartMeterId = 12345 };
            var decryptedMessage = JsonConvert.SerializeObject(smartDevice);

            _mockSmartMeterRepo
                .Setup(repo => repo.UpdateMeterData(It.IsAny<SmartDevice>()))
                .Returns((SmartDevice)null);

            // Act
            var response = _smartMeterServices.UpdateMeterServices(decryptedMessage);

            // Assert
            Assert.NotNull(response);
            Assert.Equal("SmartMeter not found", response.Message);
        }

        [Fact]
        public void UpdateMeterServices_WhenExceptionThrown_LogsErrorAndThrows()
        {
            // Arrange
            var smartDevice = new SmartDevice { SmartMeterId = 12345 };
            var decryptedMessage = JsonConvert.SerializeObject(smartDevice);

            _mockSmartMeterRepo
                .Setup(repo => repo.UpdateMeterData(It.IsAny<SmartDevice>()))
                .Throws(new Exception("Database connection error"));

            // Act & Assert
            var exception = Assert.Throws<Exception>(() => _smartMeterServices.UpdateMeterServices(decryptedMessage));
            Assert.Equal("Database connection error", exception.Message);

            _mockErrorLogRepo.Verify(repo => repo.LogError(It.Is<ErrorLogMessage>(
                log => log.ClientId == smartDevice.SmartMeterId &&
                       log.Message.Contains("Unable to access smart meter repo"))), Times.Once);
        }
    }
}
