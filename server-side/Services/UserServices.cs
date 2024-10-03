
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
                break;
                case "addUser":
                {
                    var result = _userRepo.AddUserData(userJson);
                    if (result){
                        return new UserResponse
                        {
                            Successs = true
                        };
                    }
                    else{
                        return new UserResponse
                        {
                            Successs = false
                        };
                    }
                    
                }
                break;
            }
            
            return new UserResponse
            {
                UserID = userJson.UserID,
                firstName = userJson.firstName,
                lastName = userJson.lastName,
                UserEmail = userJson.UserEmail,
                Address = userJson.Address,
                Topic = userJson.Topic,
                Successs = false
                
            };
        }

        public bool AddUser(UserData userData)
        {
            var result = _userRepo.AddUserData(userData);
            return result;
        }

    }
}
