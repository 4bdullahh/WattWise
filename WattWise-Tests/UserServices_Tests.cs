using Moq;
using Newtonsoft.Json;
using server_side.Repository.Interface;
using server_side.Repository.Models;
using server_side.Services.Interface;
using server_side.Services.Models;

namespace WattWise_Tests;

public class UserServices_Tests
{
    
    private readonly Mock<IErrorLogRepo> _mockErrorLogRepo;
    private readonly Mock<IUserServices> _mockUserServices;

    public UserServices_Tests()
    {
        _mockErrorLogRepo = new Mock<IErrorLogRepo>();
        _mockUserServices = new Mock<IUserServices>();
    }
    
      [Fact]
        public void UpdateUserServices_WhenUserExists_ReturnsUserResponse()
        {
            // Arrange
            var UserData = new UserResponse
            {
                UserID = 12345,
                UserEmail = "dummyuser@hotmail.com",
            };
            var decryptedMessage = JsonConvert.SerializeObject(UserData);
            _mockUserServices.Setup(service => service.UserOperations(decryptedMessage)).Returns(UserData);
            // Act
            var response = _mockUserServices.Object.UserOperations(decryptedMessage);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(UserData, response);
            
            _mockUserServices.Verify(repo => repo.UserOperations(decryptedMessage), Times.Once);
        }

        [Fact]
        public void UpdatUserServices_Should_NullReference_Exception()
        {
            // Arrange
            var userData = new UserData { UserID = 12345, };
            var decryptedMessage = JsonConvert.SerializeObject(userData);
            
            var expectedResponse = new UserResponse { UserID = 12345 }; 
        
            _mockUserServices.Setup(services => services.UserOperations(decryptedMessage)).Returns((UserResponse)null);
        
            // Act
            var response = _mockUserServices.Object.UserOperations(decryptedMessage);
        
            // Assert
            Assert.Null(response);
            Assert.IsNotType<UserResponse>(response);
            Assert.NotEqual(expectedResponse, response);
            Assert.Throws<NullReferenceException>(() => {
                var smartMeterId = response.UserID;
            });
            _mockUserServices.Verify(repo => repo.UserOperations(decryptedMessage), Times.Once);
        }

        [Fact]
        public void UpdateUserServices_Should_Return_Success_False_When_Repo_Returns_Null()
        {
            // Arrange
            var userData = new UserData
            {
                UserID = 12345
            };
            var decryptedMessage = JsonConvert.SerializeObject(userData);
            
            var expectedResponse = new UserResponse
            {
                UserID = 12345,
                Successs = false
            }; 
        
            _mockUserServices.Setup(services => services.UserOperations(decryptedMessage)).Returns(expectedResponse);
        
            // Act
            var response = _mockUserServices.Object.UserOperations(decryptedMessage);
        
            // Assert
            Assert.NotNull(response);
            Assert.IsType<UserResponse>(response);
            Assert.False(response.Successs);
     
            _mockUserServices.Verify(services => services.UserOperations(decryptedMessage), Times.Once);
        }
        [Fact]
        public void UpdateUserServices_Should_Return_Message_When_User_Exists_Success_False()
        {
            // Arrange
            var smartDevice = new UserData
            {
                UserID = 1001,
            };
            var decryptedMessage = JsonConvert.SerializeObject(smartDevice);
            
            var expectedResponse = new UserResponse
            {
                UserID = 1001,
                Successs = false,
                Message = "User allready exists and cannot be added again"
            }; 
        
            _mockUserServices.Setup(services => services.UserOperations(decryptedMessage)).Returns(expectedResponse);
        
            // Act
            var response = _mockUserServices.Object.UserOperations(decryptedMessage);
            
            // Assert
            Assert.NotNull(response);
            Assert.IsType<UserResponse>(response);
            Assert.Equal(expectedResponse.UserID, response.UserID);
       
            _mockUserServices.Verify(repo => repo.UserOperations(decryptedMessage), Times.Once);
        }
        
        [Theory]
        [InlineData("getId", "User Id Retrieved")]
        [InlineData("addUser", "User Added")]
        [InlineData("UpdateUser", "User Updated Successfully")]
        public void UpdateUserServices_Should_Contain_SubString_In_Message_When_Valid_Topic(string topic, string message)
        {
            // Arrange
            var userData = new UserData
            {
                UserID = 1001,
                Topic = topic,
                Message = message
            };
            var decryptedMessage = JsonConvert.SerializeObject(userData);
            
            var expectedResponse = new UserResponse
            {
                UserID = 1001,
                Topic = topic,
                Message = message
            }; 
        
            _mockUserServices.Setup(services => services.UserOperations(decryptedMessage)).Returns(expectedResponse);
        
            // Act
            var response = _mockUserServices.Object.UserOperations(decryptedMessage);
            
            // Assert
            Assert.NotNull(response);
            Assert.IsType<UserResponse>(response);
            Assert.Contains(expectedResponse.Message, response.Message);
            Assert.Equal(expectedResponse.Topic, response.Topic);
            Assert.Equal(expectedResponse, response);
            
            _mockUserServices.Verify(repo => repo.UserOperations(decryptedMessage), Times.Once);
        }
    
    
}