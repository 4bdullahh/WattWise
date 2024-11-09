using System.Diagnostics;
using server_side.Repository.Interface;
using server_side.Repository.Models;

namespace server_side.Repository;

public class PowerGridCalc :IPowerGridCalc
{
    private List<SmartDevice> smartDevices;
    private readonly ErrorLogMessage _errorLogMessage;
    
    public PowerGridCalc()
    {
        _errorLogMessage = new ErrorLogMessage();
    }

    public SmartDevice CalculateGridOutage(SmartDevice smartDevice)
    {

        double totalKwhUsed = 0;
        var random = new Random();
        double multiplier = 0.1 + random.NextDouble() * 0.1;
        double kwhCutOff = 4.0;
        
        for (int i = 0; i < smartDevices.Count; i++)
        {
            if (smartDevice.SmartMeterId == smartDevices[i].SmartMeterId)
            {
                smartDevices.Remove(smartDevices[i]);
                smartDevices.Add(smartDevice);
            }
            else
            {
                smartDevices.Add(smartDevice);
            }

            totalKwhUsed += smartDevices[i].KwhUsed + multiplier;

        }
        if (totalKwhUsed >= kwhCutOff)
        {
            smartDevice.Message = $"Power grid outage usage at {totalKwhUsed} kwh";
            return smartDevice;
        }
       
        return null;
        
    }
}