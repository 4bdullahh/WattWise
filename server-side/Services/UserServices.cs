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
                    var getMessage = server.ReceiveFrameString();
                    Console.WriteLine($"Received message: {getMessage}");
                    
                    var message = getMessage.Split(':');
                    byte[] key = Convert.FromBase64String(message[0]);
                    byte[] iv = Convert.FromBase64String(message[1]);
                    string receivedHash = message[2];
                    string receivedUser = message[3];
                    
                    byte[] encryptedMessage = Convert.FromBase64String(receivedUser);
                    string decryptedMessage = Cryptography.Cryptography.Decrypt(encryptedMessage , key, iv);
                    
                    string userHash = Cryptography.Cryptography.GenerateHash(decryptedMessage);

                    if (userHash != receivedHash)
                    {
                        Console.WriteLine("Hash doesn't match");
                    }
                    else
                    {
                        var userJson = JsonConvert.DeserializeObject<UserData>(decryptedMessage);
                        var response = HandleMessage(userJson);
                        server.SendFrame(JsonConvert.SerializeObject(response));
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
                            Hash = userData.Hash,
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
