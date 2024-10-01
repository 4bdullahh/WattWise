
using NetMQ;
using NetMQ.Sockets;
using Newtonsoft.Json;
using server_side.Service.Interface;
using server_side.Repository.Interface;
namespace server_side.Service
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
                    Console.WriteLine("Server receieved", message);

                    UserData userJson = JsonConvert.DeserializeObject<UserData>(message);

                    var response = HandleMessage(userJson);

                    var jsonResponse = JsonConvert.SerializeObject(response);

                    server.SendFrame(jsonResponse);
                }
            }

        }

        private UserResponse HandleMessage(UserData userJson)
        {

            int userId = userJson.UserID;
            
            var id = 101;

            var userData = _userRepo.GetById(101);

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

    }
}
