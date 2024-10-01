using Newtonsoft.Json;
using NetMQ;
using NetMQ.Sockets;

using (var client = new RequestSocket())
{
    client.Connect("tcp://localhost:5555");
    for (int i = 0; i < 10; i++)
    {
        var userData = new UserModel { UserID = 0, UserEmail = "fred@hotmail.com" };
        var jsonRequest = JsonConvert.SerializeObject(userData);
        client.SendFrame(jsonRequest);
        Console.WriteLine("Sending UserData: ", jsonRequest);

        var message = client.ReceiveFrameString();

        var jsonResponse = JsonConvert.DeserializeObject<UserModel>(message);
        Console.WriteLine("Received: ", jsonResponse);

    }
}





