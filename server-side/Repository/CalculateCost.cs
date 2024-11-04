using server_side.Repository.Interface;

namespace server_side.Repository;

public class CalculateCost : ICalculateCost
{
    private const double StandingCharge = 0.60; 
    private const double CostPerKwh = 0.24;

    public SmartDevice getCurrentBill(SmartDevice modelData)
    {
        /*
         * These standard rates are taken from the below website
         * https://www.smartenergygb.org/smart-living/smart-energy-tips/what-is-the-price-cap-and-will-it-affect-your-energy-bills?gclid=49e98d7f1cc31a9fe942ea67df695dcb&gclsrc=3p.ds&msclkid=49e98d7f1cc31a9fe942ea67df695dcb#whatiscap-epg-rev3:~:text=What%20is%20the%20current%20price%20of%20gas%20and%20electricity%20per%20kWh%3F
         */
       
        string customerType = modelData.CustomerType;
        double averageMinuteUsage = GetAverageMinuteUsage(customerType);

        modelData.StandingCharge = StandingCharge;
        modelData.CostPerKwh = CostPerKwh;

        var curTime = DateTime.Now;
        DateTime billingStartDate = new DateTime(curTime.Year, curTime.Month, 1);
        modelData.KwhUsed = CalculateKwh(curTime, averageMinuteUsage, billingStartDate);

        var totalCost = CalculateRates(modelData.KwhUsed, StandingCharge, CostPerKwh, averageMinuteUsage);
        modelData.CurrentMonthCost = totalCost;

        return modelData;
    }
    private double GetAverageDailyUsage(string customerType)
    {
        return customerType switch
        {
            "Large Household" => 15.0,
            "Average Household" => 10.0,
            "Small Household" => 8.0,
            _ => 0.0,
        };
    }

    private double GetAverageMinuteUsage(string customerType)
    {
        double averageDailyUsage = GetAverageDailyUsage(customerType);
        return averageDailyUsage / (24 * 60);
    }

    private double CalculateKwh(DateTime currentTime, double averageMinuteUsage, DateTime startDate)
    {
        TimeSpan timeSpan = currentTime - startDate;
        double totalMinutes = timeSpan.TotalMinutes;
        return averageMinuteUsage * totalMinutes;
    }

    public double CalculateRates(double kwhUsed, double standingCharge, double costPerKwH, double averageDailyUsage)
    {
        double currentEnergyCost = costPerKwH * kwhUsed;

        int daysInMonth = DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month);
        double totalStandingCharge = standingCharge * daysInMonth;

        double totalCost = currentEnergyCost + totalStandingCharge;
        return totalCost;

    }
}
