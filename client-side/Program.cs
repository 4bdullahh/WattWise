using System;
using Newtonsoft.Json;
using NetMQ;
using NetMQ.Sockets;

namespace client_side
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var client = new RequestSocket())
            {
                client.Connect("tcp://localhost:5555");
                for (int i = 0; i < 10; i++)
                {
                    var userData = new UserModel { UserID = 101, Topic = "getId" };
                    var jsonRequest = JsonConvert.SerializeObject(userData);
                    
                    // We might use this later for Electron
                    //var topic = userData.Topic;
                    
                    client.SendFrame(jsonRequest);
                    Console.WriteLine($"Sending UserData: {jsonRequest}");
                    Thread.Sleep(500);

                    var message = client.ReceiveFrameString();
                    // We might need this for Electron
                    // var jsonResponse = JsonConvert.DeserializeObject<UserModel>(message);
                    Console.WriteLine($"Received: {message}");
                }
            }
        }
    }
}
