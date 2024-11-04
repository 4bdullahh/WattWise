
using client_side.Services;
using client_side.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using server_side.Repository;
using server_side.Repository.Interface;
using server_side.Services;
using server_side.Services.Interface;

namespace client_side
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {

            var serviceCollection = new ServiceCollection();
            serviceCollection.AddScoped<IMessagesServices, MessagesServices>();
            serviceCollection.AddScoped<IClientServices, ClientServices>();
            serviceCollection.AddScoped<ICalculateCostClient, CalculateCostClient>();
            serviceCollection.AddScoped<IFolderPathServices, FolderPathServices>();
            serviceCollection.AddScoped<ISmartMeterRepo, SmartMeterRepo>();
            serviceCollection.AddScoped<ISaveData, SaveData>();
            serviceCollection.AddScoped<ICalculateCost, CalculateCost>();
            var serviceProvider = serviceCollection.BuildServiceProvider();
            var clientService = serviceProvider.GetService<IClientServices>();
            clientService.StartClient();
            //await clientService.ElectronServerAsync();

        }

    }
}