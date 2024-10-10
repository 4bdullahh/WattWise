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

        public bool AddUser(UserData userData)
        {
            var result = _userRepo.AddUserData(userData);
            return result;
        }

        public void ReceiveMessageServices()
        {
            using (var server = new RouterSocket("@tcp://*:5544"))
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
                default:
                    {
                        return new UserResponse
                        {
                            Successs = false,
                            Message = ""
                        };
                    }
            }
        }
    }
}