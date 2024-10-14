using NetMQ;
using NetMQ.Sockets;
using Newtonsoft.Json;
using server_side.Repository.Interface;
using server_side.Services.Interface;
using System.Text;

namespace server_side.Services
{
    public class UserService : IUserServices 
    {
        private readonly IUserMessageRepo _userRepo;

        public UserService(IUserMessageRepo userRepo)
        {
            _userRepo = userRepo;
        }
        public void ReceiveMessageServices()
        {
            using (var server = new RouterSocket("@tcp://*:5555"))
            using (var poller = new NetMQPoller())
            {
                while (true)
                {
                    var recievedMessage = server.ReceiveMultipartMessage();
                    Console.WriteLine($"Received message: {recievedMessage}");
                    var clientAddress = recievedMessage[0];

                    byte[] key = Convert.FromBase64String(Encoding.UTF8.GetString(recievedMessage[3].ToByteArray()));
                    byte[] iv = Convert.FromBase64String(Encoding.UTF8.GetString(recievedMessage[4].ToByteArray()));
                    string receivedHash = Encoding.UTF8.GetString(recievedMessage[5].Buffer);
                    string receivedUser = Encoding.UTF8.GetString(recievedMessage[6].Buffer);
                    byte[] encryptedMessage = Convert.FromBase64String(receivedUser);
                    string decryptedMessage = Cryptography.Cryptography.Decrypt(encryptedMessage, key, iv);
                    string userHash = Cryptography.Cryptography.GenerateHash(decryptedMessage);

                    //This is for test when the data is temperaded
                    /*
                    UserData tempered = JsonConvert.DeserializeObject<UserData>(decryptedMessage);
                    tempered.UserID = 1000;
                    var temperedJson = JsonConvert.SerializeObject(tempered);
                    string userHash = Cryptography.Cryptography.GenerateHash(temperedJson);*/

                    if (userHash != receivedHash)
                    {
                        Console.WriteLine("Hash doesn't match");
                    }
                    else
                    {
                        var messageToClient = new NetMQMessage();
                        messageToClient.Append(clientAddress);
                        messageToClient.AppendEmptyFrame();
                        UserData userJson = JsonConvert.DeserializeObject<UserData>(decryptedMessage);
                        
                        var response = HandleMessage(userJson);
                        
                        
                        var jsonResponse = JsonConvert.SerializeObject(response);
                        messageToClient.Append(jsonResponse);
                        server.SendMultipartMessage(messageToClient);
                        Console.WriteLine($"Sending to Client: {jsonResponse}");
                    }
                }
            }
        }

        private UserResponse HandleMessage(UserData userJson)
        {
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
                            firstName = userData.firstName,
                            lastName = userData.lastName,
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
                        if (userData)
                        {
                            return new UserResponse
                            {
                                UserID = userJson.UserID,
                                firstName = userJson.firstName,
                                Successs = true,
                                Message = "User Added"
                            };
                        }
                        return new UserResponse { Successs = false, Message = "Error adding user" };  // Add this return

                    }
                
                case "UpdateUser":
                {
                    var existingUserData = _userRepo.GetById(userJson.UserID);

                    if (existingUserData != null)
                    {
                        var updateUser = _userRepo.UpdateUserData(existingUserData);

                        if (updateUser)
                        {
                            return new UserResponse
                            {
                                UserID = userJson.UserID,
                                firstName = userJson.firstName,
                                lastName = userJson.lastName,
                                UserEmail = userJson.UserEmail,
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
                        return new UserResponse
                        {
                            Successs = false,
                            Message = "Invalid Topic Request from client"
                        };
                    }
            }

        }
    }
}