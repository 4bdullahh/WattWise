
using NetMQ;
using NetMQ.Sockets;
using Newtonsoft.Json;
using server_side.Services.Interface;
using server_side.Repository.Interface;

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
            using (var server = new ResponseSocket("@tcp://*:5555"))
            {

                while (true)
                {
                    var message = server.ReceiveFrameString();
                    // Acknowledge Recieved Message
                    Console.WriteLine($"Send Recieve {message}");

                    UserData userJson = JsonConvert.DeserializeObject<UserData>(message);

                    var response = HandleMessage(userJson);

                    var jsonResponse = JsonConvert.SerializeObject(response);

                    server.SendFrame(jsonResponse);
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
