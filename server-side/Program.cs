using Microsoft.Extensions.DependencyInjection;
using server_side.Services.Interface;
using server_side.Repository.Interface;
using server_side.Services;
using server_side.Repository;
using server_side.Controller;

namespace server_side
{
    class Program
    {
        static void Main(string[] args)
        {
            var services = new ServiceCollection();
            services.AddScoped<IMessageServices, MessageService>();
            services.AddScoped<IUserServices, UserServices>();
            services.AddScoped<IUserMessageRepo, UserMessageRepo>();
            services.AddScoped<ISmartMeterServices, SmartMeterServices>();
            services.AddScoped<ISmartMeterRepo, SmartMeterRepo>();
            services.AddScoped<ICalculateCost, CalculateCost>();
            services.AddSingleton<CostUpdateService>();
            services.AddHostedService<CostUpdateService>();
            services.AddScoped<ISaveData, SaveData>();
            services.AddScoped<IFolderPathServices, FolderPathServices>();
            services.AddScoped<IHashHandle, HashHandle>();

            services.AddSingleton<MessageController>();

            var serviceProvider = services.BuildServiceProvider();


            var messageController = serviceProvider.GetService<MessageController>();
            messageController.ReceiveMessage();

            Console.WriteLine("Message receiving started. Press any key to exit...");
            Console.ReadKey();
        }
    }

}
