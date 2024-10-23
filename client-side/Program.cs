
using client_side.Services;
using client_side.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace client_side
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {

            var serviceCollection = new ServiceCollection();
            serviceCollection.AddScoped<IMessagesServices, MessagesServices>();
            serviceCollection.AddScoped<IClientServices, ClientServices>();

            var serviceProvider = serviceCollection.BuildServiceProvider();
            var clientService = serviceProvider.GetService<IClientServices>();
            await clientService.ElectronServerAsync();

        }

    }
}