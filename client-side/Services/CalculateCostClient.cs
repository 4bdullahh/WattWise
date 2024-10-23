using client_side.Services.Interfaces;

namespace client_side.Services;

public class CalculateCostClient :ICalculateCostClient
{
    public CalculateCostClient()
    {
        
    }

    public SmartDevice getRandomCost(SmartDevice modelData)
    {
        double standingCharge = 0.60;
        double CostPerKwh = 0.24;
        string customerType = "";
        var curTime = DateTime.Now;
        
        /*
         * These low to high use rates are taken from Ofgem.gov.uk
         * https://www.ofgem.gov.uk/average-gas-and-electricity-usage#:~:text=high%20energy%20use.-,Typical%20values,-The%20energy%20price
         */
        
        double lowUsePerHour = 0.20;
        double medUsePerHour = 0.30;
        double highUsePerHour = 0.46;
        
        switch (customerType)
        {
            case "Large Household":
            {
                modelData.EnergyPerKwH = lowUsePerHour;
            }
                break;
            case "Average Household":
            {
                modelData.EnergyPerKwH = medUsePerHour;
            }
                break;
            case "Small Household":
            {
                modelData.EnergyPerKwH = highUsePerHour;
            }
                break;
        }
        
        var ratesCalculated = CalculateRates(
            curTime, 
            modelData.EnergyPerKwH, 
            standingCharge,
            CostPerKwh);
                
        modelData.CurrentMonthCost = ratesCalculated;
        
        return modelData;
    }

    public double CalculateRates(DateTime currentTime, double usePerHour, double standingCharge, double costPerKwh)
    { 
        usePerHour = usePerHour * currentTime.Hour;
        var currentDayCost = costPerKwh * usePerHour;

        var daysPassed = currentTime.Day;
        var currentMonthCost = currentDayCost * daysPassed;
        var calcStandingCharge =  standingCharge * daysPassed;
        var totalCost = currentMonthCost + calcStandingCharge;
        
        return totalCost;
    }
    
    
}