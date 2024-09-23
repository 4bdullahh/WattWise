using NetMQ;
using NetMQ.Sockets;
 
public class EchoClient
{
    public void Start()
    {
        using (var client = new RequestSocket(">tcp://localhost:12345")) 
        {
            Console.Write("Enter a message: ");
            string userInput = Console.ReadLine();
            client.SendFrame(userInput);
 
            string response = client.ReceiveFrameString();
            Console.WriteLine("Server response: " + response);
        }
    }
}