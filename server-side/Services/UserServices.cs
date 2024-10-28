﻿using Newtonsoft.Json;
using server_side.Repository.Interface;
using server_side.Services.Interface;

namespace server_side.Services
{
    public class UserServices : IUserServices
    {
        private readonly IUserMessageRepo _userRepo;
        
        public UserServices(IUserMessageRepo userRepo)
        {
            _userRepo = userRepo;
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
                                UserID = userJson.UserID,
                                FirstName = userJson.firstName,
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
                                    UserID = userJson.UserID,
                                    FirstName = userJson.firstName,
                                    LastName = userJson.lastName,
                                    UserEmail = userJson.UserEmail,
                                    Successs = true,
                                    Message = "User Updated Successfully"
                                };
                            }
                            return new UserResponse
                            {
                                Successs = false,
                                Message = "Error updating user"
                            };
                        }
                        return new UserResponse
                        {
                            Successs = false,
                            Message = "User doesn't exist"
                        };
                    }
                    default:
                    {
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
                Console.WriteLine($"We could not process this message: {e.Message}");
                throw;
            }

        }
    }
    
}

