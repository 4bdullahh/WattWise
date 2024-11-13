using System.Net;
using server_side.Repository.Interface;
using server_side.Repository.Models;

namespace server_side.Repository;

public class CalculateCost : ICalculateCost
{
    private const double StandingCharge = 0.60; 
    private const double CostPerKwh = 0.24;
    private readonly IErrorLogRepo _errorLogRepo;
    private readonly ErrorLogMessage _errorLogMessage;

    public CalculateCost(IErrorLogRepo errorLogRepo)
    {
        _errorLogRepo = errorLogRepo;
        _errorLogMessage = new ErrorLogMessage();
    }
    public SmartDevice getCurrentBill(SmartDevice modelData)
    {
        /*
         * These standard rates are taken from the below website
         * https://www.smartenergygb.org/smart-living/smart-energy-tips/what-is-the-price-cap-and-will-it-affect-your-energy-bills?gclid=49e98d7f1cc31a9fe942ea67df695dcb&gclsrc=3p.ds&msclkid=49e98d7f1cc31a9fe942ea67df695dcb#whatiscap-epg-rev3:~:text=What%20is%20the%20current%20price%20of%20gas%20and%20electricity%20per%20kWh%3F
         */
        
        try
        {
            if (modelData == null)
            {
                Console.WriteLine($"Error in getCurrentBill: {modelData} is null");
                return new SmartDevice { KwhUsed = 0, CurrentMonthCost = 0 };
            }
            if (string.IsNullOrEmpty(modelData.CustomerType) || GetAverageDailyUsage(modelData.CustomerType) == 0)
            {
                modelData.KwhUsed = 0;
                modelData.CurrentMonthCost = 0;
                return modelData;
            }

            string customerType = modelData.CustomerType;
            double averageMinuteUsage = GetAverageMinuteUsage(customerType);

            modelData.StandingCharge = StandingCharge;
            modelData.CostPerKwh = CostPerKwh;

            var curTime = DateTime.Now;
            DateTime billingStartDate = new DateTime(curTime.Year, curTime.Month, 1);
            modelData.KwhUsed = CalculateKwh(curTime, averageMinuteUsage, billingStartDate);

            var totalCost = CalculateRates(modelData.KwhUsed, StandingCharge, CostPerKwh, averageMinuteUsage);
            modelData.CurrentMonthCost = totalCost;
            modelData.Message = $"Cost calculation for {customerType}: {totalCost}";
            return modelData;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in getCurrentBill: {ex.Message}");
            modelData.KwhUsed = 0;
            modelData.CurrentMonthCost = 0;
            return modelData;
        }
    }
    private double GetAverageDailyUsage(string customerType)
    {
        
        try
        {
            // Uncomment this for testing log error
           // throw new Exception("The method or operation is not implemented.");
            return customerType switch
            {
                "Small NGO" => 5.0,
                "Small Household" => 8.0,
                "Small Business" => 11.0,
                "Small Industrial" => 20.0,
                "Small Public Service" => 6.0,
                "Average NGO" => 10.0,
                "Average Household" => 10.0,
                "Average Business" => 18.0,
                "Average Industrial" => 25.0,
                "Average Public Service" => 9.0,
                "Large NGO" => 15.0,
                "Large Household" => 15.0,
                "Large Business" => 20.0,
                "Large Industrial" => 30.0,
                "Large Public Service" => 12.0,
                _ => 0.0
            };
        }
        catch (Exception e)
        {
            _errorLogMessage.Message = $"Server: ClientID {_errorLogMessage.ClientId} Customer type invalid {e.Message} : {DateTime.UtcNow}";
            Console.WriteLine($"{_errorLogMessage.Message}");
            _errorLogRepo.LogError(_errorLogMessage);
            throw;
        }
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
        return Math.Round(totalCost, 2);
    }
}
