namespace client_side.Models;

public class SmartDeviceClient
{
    public int SmartMeterId { get; set; }
    public double EnergyPerKwH { get; set; }
    public double CurrentMonthCost { get; set; }
    public double KwhUsed { get; set; }
    public double StandingCharge { get; set; }
    public double CostPerKwH { get; set; }
    public string CustomerType { get; set; }

    
    
}