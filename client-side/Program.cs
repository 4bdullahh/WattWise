using Newtonsoft.Json;
using NetMQ;
using NetMQ.Sockets;

namespace client_side
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var client = new PublisherSocket())
            {
                Console.WriteLine("Publisher socket binding...");
                client.Options.SendHighWatermark = 10;
                client.Bind("tcp://*:5555");
                for (var i = 0; i < 10; i++)
                {

                    var userData = new UserModel { UserID = 1, UserEmail = "fred@hotmail.com", Topic = "getId" };
                    var jsonRequest = JsonConvert.SerializeObject(userData);

                    var topic = userData.Topic;

                    switch (topic)
                    {
                        case "getId":
                            {
                                client.SendMoreFrame(topic).SendFrame(jsonRequest);
                                Console.WriteLine("Sending UserData: ", jsonRequest);
                            }
                            break;
                        case "addUser":
                            {
                                client.SendMoreFrame(topic).SendFrame(jsonRequest);
                                Console.WriteLine("Sending UserData: ", jsonRequest);
                            }
                            break;
                    }
                    Thread.Sleep(500);

                    var message = client.ReceiveFrameString();
                    var jsonResponse = JsonConvert.DeserializeObject<UserModel>(message);
                    Console.WriteLine("Received: ", jsonResponse);
                }
            }
        }
    }
}
