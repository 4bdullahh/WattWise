
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
            using (var server = new SubscriberSocket())
            {
                server.Options.ReceiveHighWatermark = 10;
                server.Connect("tcp://localhost:5555");
                server.Subscribe("");
                while (true)
                {
                    var topic = server.ReceiveFrameString();


                    switch (topic)
                    {
                        case "getId":
                            {
                                UserData userJson = JsonConvert.DeserializeObject<UserData>(topic);
                                var response = getUserId(userJson.UserID);
                                Console.WriteLine($"Recieved: {topic}");

                            }
                            break;

                        case "addUser":
                            {
                                UserData userJson = JsonConvert.DeserializeObject<UserData>(topic);
                                var response = addUserServices(userJson);
                                Console.WriteLine($"Recieved: {topic}");

                            }
                            break;
                    }


                    var jsonResponse = JsonConvert.SerializeObject(response);
                    server.SendFrame(jsonResponse);
                    Console.WriteLine($"Sending: {jsonResponse}");

                }
            }
        }

        private UserResponse getUserId(int userId)
        {

            var userData = _userRepo.GetById(userId);

            if (userData == null)
            {
                return new UserResponse { UserID = 0, Message = "User could not be found" };
            }

            return new UserResponse
            {
                firstName = userData.firstName,
                lastName = userData.lastName,
                UserEmail = userData.UserEmail
            };
        }

        private UserResponse addUserServices(UserData userData)
        {
            var addUser = _userRepo.AddUserData(userData);
            if (addUser == null)
            {
                return new UserResponse { Message = "User Could not be added" };
            }

            return new UserResponse
            {
                UserEmail = addUser.UserEmail,
                firstName = addUser.firstName,
                lastName = addUser.lastName,
                Message = "User Found"
            };
        }
    }
}
