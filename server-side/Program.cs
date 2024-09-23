// Program.cs

namespace SmartMeter
{
    class Program
    {
        static void Main(string[] args)
        {
            var messageRepository = new UserMessageRepo("tcp://*:12345", "tcp://localhost:12345");
            var messageController = new MessageController(messageRepository);


        }
    }
}

