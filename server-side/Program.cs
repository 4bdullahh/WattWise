using NetMQ;
using Microsoft.Extensions.DependencyInjection;

namespace SmartMeter
{
    class Program
    {
        static void Main(string[] args)
        {
            var services = new ServiceCollection();
            services.AddScoped<IUserServices, UserService>();
            services.AddScoped<IUserMessageRepo, UserMessageRepo>();
            services.AddSingleton<MessageController>();

            var serviceProvider = services.BuildServiceProvider();

            var messageController = serviceProvider.GetService<MessageController>();
            messageController.ReceiveMessage();

            Console.WriteLine("Message receiving started. Press any key to exit...");
            Console.ReadKey();
        }
    }

}

