using System;
using System.Threading.Tasks;
 
public class Program
{
    public static void Main(string[] args)
    {
        Task.Run(() => 
        {
            var server = new EchoServer();
            server.Start();
        });
 
        Task.Delay(1000).Wait(); 
 
        var client = new EchoClient();
        client.Start();
    }
}