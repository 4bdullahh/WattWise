using client_side.Services.Interfaces;
using client_side.Models;
namespace client_side.Services;

public class CalculateCostClient :ICalculateCostClient
{
    public SmartDeviceClient getRandomCost(SmartDeviceClient modelData, string customerType)
    {
        /*
         * All rates of usage per hour are taken from Ofgem.gov.uk which are averages
         * https://www.ofgem.gov.uk/average-gas-and-electricity-usage#:~:text=high%20energy%20use.-,Typical%20values,-The%20energy%20price
         */

        //can go into detail energy based on time of day
        var generateAvgUse = new Random();
        double max = 0.1;
        double min = 0.05;
        double multiplier = generateAvgUse.NextDouble() * (max - min) + min;

        /*
         * HouseHolds
         */
        //1800Kwh per year in small household 1 - 2 people
        //2700Kwh per year in  medium household 2 - 3 people
        //4100Kwh per year in  large household 4+ people
        
        //The below are the above costs but divided per hour usage
        double lowUsePerHour = 0.20;
        double medUsePerHour = 0.30;
        double highUsePerHour = 0.46;
        double lowBusinessPerHour = 0.60;
        double medBusinessPerHour = 0.80;
        double highBusinessPerHour = 0.90;
        double latestUsePerHour;

        try
        {
            switch (customerType)
            {
                case "Large Household":
                {
                    latestUsePerHour = AddPriceFluctuation(modelData.EnergyPerKwH, highUsePerHour, multiplier);
                }
                    break;
                case "Average Household":
                {
                    latestUsePerHour = AddPriceFluctuation(modelData.EnergyPerKwH, medUsePerHour, multiplier);
                }
                    break;
                case "Small Household":
                {
                    latestUsePerHour = AddPriceFluctuation(modelData.EnergyPerKwH, lowUsePerHour, multiplier);
                }
                    break;case "Small Business":
                {
                    latestUsePerHour = AddPriceFluctuation(modelData.EnergyPerKwH, lowBusinessPerHour, multiplier * 0.1);
                }
                    break;case "Average Business":
                {
                    latestUsePerHour = AddPriceFluctuation(modelData.EnergyPerKwH, medBusinessPerHour, multiplier * 0.15);
                }
                    break;case "Large Business":
                {
                    latestUsePerHour = AddPriceFluctuation(modelData.EnergyPerKwH, highBusinessPerHour, multiplier * 0.20);
                }
                    break;
                default:
                {
                    latestUsePerHour = 0;
                }
                    break;
            }

            modelData.EnergyPerKwH = latestUsePerHour;
            modelData.CustomerType = customerType;

            return modelData;
        }
        catch (Exception e)
        {
            throw new Exception(e.Message);
        }
        
    }

    
    public double AddPriceFluctuation(double modelEnergyData, double usePerHour, double multiplier)
    {
        try
        {
            double energyFluctuation = usePerHour * multiplier;
            modelEnergyData = usePerHour + energyFluctuation;
            return modelEnergyData;
        }
        catch (Exception e)
        {
            throw new Exception(e.Message);
        }
        
    }
    
    
}