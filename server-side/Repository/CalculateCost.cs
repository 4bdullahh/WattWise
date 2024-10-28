using server_side.Repository.Interface;

namespace server_side.Repository;

public class CalculateCost : ICalculateCost
{
    
    public CalculateCost()
    {
        
    }

    public SmartDevice getCurrentBill(SmartDevice modelData)
    {
        /*
         * These standard rates are taken from the below website
         * https://www.smartenergygb.org/smart-living/smart-energy-tips/what-is-the-price-cap-and-will-it-affect-your-energy-bills?gclid=49e98d7f1cc31a9fe942ea67df695dcb&gclsrc=3p.ds&msclkid=49e98d7f1cc31a9fe942ea67df695dcb#whatiscap-epg-rev3:~:text=What%20is%20the%20current%20price%20of%20gas%20and%20electricity%20per%20kWh%3F
         */
        
        var curTime = DateTime.Now;
        double standingCharge = 0.60;
        double CostPerKwh = 0.24;
        
        modelData.StandingCharge = standingCharge;
        modelData.CostPerKwh = CostPerKwh;
      
        
        
        var ratesCalculated = CalculateRates(
            curTime, 
            modelData.EnergyPerKwH, 
            modelData.StandingCharge,
            modelData.CostPerKwh);
                
        modelData.CurrentMonthCost = ratesCalculated;
        
        return modelData;
    }

    public double CalculateRates(DateTime currentTime, double kwhPerHour, double standingCharge, double costPerKwh)
    {
        try
        {
           
            var currentDayCost = costPerKwh * (kwhPerHour * currentTime.Hour);
            var daysPassedInMonth = currentTime.Day;
            var currentMonthCost = currentDayCost * daysPassedInMonth;
            var calcStandingCharge = standingCharge * daysPassedInMonth;
            var totalCost = currentMonthCost + calcStandingCharge;
            var roundedTotalCost = Math.Round(totalCost, 2);
            
            return roundedTotalCost;
        }
        catch (Exception e)
        {
            throw new Exception("Calculate Rate Error", e);
        }
        
    }
}