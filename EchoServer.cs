using NetMQ;
using NetMQ.Sockets;
 
public class EchoServer
{
    public void Start()
    {
        using (var server = new ResponseSocket("@tcp://*:12345")) 
        {
            Console.WriteLine("Server started, waiting for clients...");
 
            while (true)
            {
                string message = Console.ReadLine();
                Console.WriteLine("Received: " + message);
                server.SendFrame(message); 
            }
        }
    }
}