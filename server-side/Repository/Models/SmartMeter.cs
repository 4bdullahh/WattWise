public class SmartDevice
{
    public int SmartMeterId { get; set; }
    public double CostPerKwh { get; set; }
    public double CurrentMonthCost { get; set; }
    public double EnergyPerKwH { get; set; }
    public double KwhUsed { get; set; }
    public double StandingCharge { get; set; }
    public double CostPerKwH { get; set; }
    public string CustomerType { get; set; }
    public UserData? UserData { get; set; } 
    public string Message { get; set; }
    
    public List<int> clientList { get; set; }
}