using Newtonsoft.Json;
using server_side.Repository.Interface;
using server_side.Repository.Models;
using server_side.Services.Interface;

namespace server_side.Services
{
    public class UserServices : IUserServices
    {
        private readonly IUserMessageRepo _userRepo;
        private readonly IErrorLogRepo _errorLogRepo;
        private readonly ErrorLogMessage _errorLogMessage;

        public UserServices(IUserMessageRepo userRepo, IErrorLogRepo errorLogRepo)
        {
            _userRepo = userRepo;
            _errorLogRepo = errorLogRepo;
            _errorLogMessage = new ErrorLogMessage();
        }

        public UserResponse UserOperations(string decryptedMessage)
        {
            try
            {
                UserData userJson = JsonConvert.DeserializeObject<UserData>(decryptedMessage);

                var topic = userJson.Topic;
                switch (topic)
                {
                    case "getId":
                    {
                        int userId = userJson.UserID;
                        var userData = _userRepo.GetById(userId);
                        if (userData == null)
                        {
                            return new UserResponse { Successs = false };
                        }

                        return new UserResponse
                        {
                            Successs = true,
                            UserID = userData.UserID,
                            FirstName = userData.firstName,
                            LastName = userData.lastName,
                            UserEmail = userData.UserEmail,
                            Address = userData.Address,
                            Topic = userData.Topic,
                        };
                    }
                    case "addUser":
                    {
                        var isExisting = _userRepo.GetById(userJson.UserID);

                        if (isExisting != null)
                        {
                            return new UserResponse
                            {
                                Successs = false,
                                Message = "User allready exists and cannot be added again"
                            };
                        }

                        var userData = _userRepo.AddUserData(userJson);
                        if (userData != null)
                        {
                            return new UserResponse
                            {
                                UserID = userData.UserID,
                                FirstName = userData.firstName,
                                Successs = true,
                                Message = "User Added"
                            };
                        }

                        return new UserResponse { Successs = false, Message = "Error adding user" }; // Add this return
                    }

                    case "UpdateUser":
                    {
                        var existingUserData = _userRepo.GetById(userJson.UserID);

                        if (existingUserData != null)
                        {
                            var updateUser = _userRepo.UpdateUserData(userJson);

                            if (updateUser != null)
                            {
                                return new UserResponse
                                {
                                    UserID = updateUser.UserID,
                                    FirstName = updateUser.firstName,
                                    LastName = updateUser.lastName,
                                    UserEmail = updateUser.UserEmail,
                                    Successs = true,
                                    Message = "User Updated Successfully"
                                };
                            }
                            else
                            {
                                return new UserResponse
                                {
                                    Successs = false,
                                    Message = "Error updating user"
                                };
                            }
                        }
                        else
                        {
                            return new UserResponse
                            {
                                Successs = false,
                                Message = "User doesn't exist"
                            };
                        }

                    }
                    default:
                    {
                        _errorLogMessage.Message = $"Server: ClientID {_errorLogMessage.ClientId} Client sent invalid topic : {DateTime.UtcNow}";
                        Console.WriteLine($"{_errorLogMessage.Message}");
                        _errorLogRepo.LogError(_errorLogMessage);
                        
                        return new UserResponse
                        {
                            Successs = false,
                            Message = "Invalid Topic Request from client"
                        };
                        
                    }

                }
            }
            catch (Exception e)
            {
                _errorLogMessage.Message = $"Server: ClientID {_errorLogMessage.ClientId} UserServices could not process action : {e.Message} : {DateTime.UtcNow}";
                Console.WriteLine($"{_errorLogMessage.Message} {e.Message}");
                _errorLogRepo.LogError(_errorLogMessage);
                throw;
            }
        }
    }
}

