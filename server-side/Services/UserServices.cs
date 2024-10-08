
using NetMQ;
using NetMQ.Sockets;
using Newtonsoft.Json;
using server_side.Services.Interface;
using server_side.Repository.Interface;
using System.ServiceModel.Channels;

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
                poller.RunAsync();
                
                while (true)
                {
                    var recievedMessage = server.ReceiveFrameString();

                    Console.WriteLine($"Send Recieve {recievedMessage}");
  
                    var clientAddress = recievedMessage[0];
                    var clientMessage = recievedMessage[2].ToString();

                    var messageToClient = new NetMQMessage();
                    messageToClient.Append(clientAddress);
                    messageToClient.AppendEmptyFrame();

                    UserData userJson = JsonConvert.DeserializeObject<UserData>(clientMessage);

                    var response = HandleMessage(userJson);

                    var jsonResponse = JsonConvert.SerializeObject(response);     

                    messageToClient.Append(jsonResponse);
                    server.SendMultipartMessage(messageToClient);
                    
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
                        var userData = _userRepo.AddUserData(userJson);
                        if (userData)
                        {
                            return new UserResponse
                            {
                                Successs = true,
                                Message = "User Added"
                            };
                        }
                        else
                        {
                            return new UserResponse
                            {
                                Successs = false,
                                Message = "User could not be added"
                            };
                        }

                    }
                default:{
                        return new UserResponse
                        {
                            Successs = false,
                            Message = ""
                        };
                    }
            }   
            
        }

        public bool AddUser(UserData userData)
        {
            var result = _userRepo.AddUserData(userData);
            return result;
        }

    }
}
