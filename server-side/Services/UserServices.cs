
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
                    
                    _userRepo.TestListToJson();

                    var response = HandleMessage(userJson);

                    var jsonResponse = JsonConvert.SerializeObject(response);

                    server.SendFrame(jsonResponse);
                }
            }

        }

        private UserResponse HandleMessage(UserData userJson)
        {

            int userId = userJson.UserID;
            var userData = _userRepo.GetById(103);

            if (userData == null)
            {
                return new UserResponse { UserID = 0, UserEmail = "User not found" };
            }

            return new UserResponse
            {
                firstName = userData.firstName,
                lastName = userData.lastName,
                UserEmail = userData.UserEmail
            };
        }

        public bool AddUser(UserData userData)
        {
            var result = _userRepo.AddUserData(userData);
            return result;
        }

    }
}
