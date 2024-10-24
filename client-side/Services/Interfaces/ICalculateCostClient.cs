using client_side.Models;

namespace client_side.Services.Interfaces;

public interface ICalculateCostClient
{
    SmartDeviceClient getRandomCost(SmartDeviceClient modelData, string customerType);
}