namespace server_side.Repository.Interface;

public interface ICalculateCost
{
    SmartDevice GetCurrentBill(double EnergyPerKwH);
    
}