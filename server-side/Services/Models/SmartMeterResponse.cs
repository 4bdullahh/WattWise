namespace server_side.Services.Models;

public class SmartMeterResponse
{
    public int SmartMeterID { get; set; }
    public double EnergyPerKwH { get; set; }
    public double CurrentMonthCost { get; set; }
    public string Message { get; set; }
}