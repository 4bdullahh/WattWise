using client_side.Services;
using client_side.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using server_side.Services.Interface;
using server_side.Services;

namespace client_side
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddScoped<IMessagesServices, MessagesServices>();
            serviceCollection.AddScoped<IClientServices, ClientServices>();
            serviceCollection.AddScoped<IFolderPathServices, FolderPathServices>();
            serviceCollection.AddScoped<ICalculateCostClient, CalculateCostClient>();

            var serviceProvider = serviceCollection.BuildServiceProvider();
            var clientService = serviceProvider.GetService<IClientServices>();
            clientService.StartClient();
            //await clientService.ElectronServerAsync();
        }
    }
}