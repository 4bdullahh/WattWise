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

        var generateAvgUse = new Random();
        double max = 0.1;
        double min = 0.05;
        double multiplier = generateAvgUse.NextDouble() * (max - min) + min;

        /* Different types of customer:
            * NGO - Non-Gov Organisations have discount in their energy prices
            * HouseHolds
            * Business
            * Industrial
            * Public Sector
         */
        
        //The below are the above costs but divided per hour usage
        var usageRates = new Dictionary<string, double>
        {
            { "Small NGO", 0.10 },
            { "Small Household", 0.15 },
            { "Small Business", 0.30 },
            { "Small Industrial", 0.50 },
            { "Small Public Service", 0.12 },
            { "Average NGO", 020},
            { "Average Household", 0.20},
            { "Average Business", 0.40 },
            { "Average Industrial", 0.60 },
            { "Average Public Service", 0.18 },
            { "Large NGO", 0.20 },
            { "Large Business", 0.50 },
            { "Large Household", 0.25},
            { "Large Industrial", 0.70 },
            { "Large Public Service", 0.24}
        };

        try
        {
            double latestUsePerHour = customerType switch
            {
                "Small NGO" => AddPriceFluctuation(modelData.EnergyPerKwH, usageRates["Small NGO"], multiplier),
                "Small Household" => AddPriceFluctuation(modelData.EnergyPerKwH, usageRates["Small Household"], multiplier),
                "Small Business" => AddPriceFluctuation(modelData.EnergyPerKwH, usageRates["Small Business"], multiplier * 0.1),
                "Small Industrial" => AddPriceFluctuation(modelData.EnergyPerKwH, usageRates["Small Industrial"], multiplier * 0.15),
                "Small Public Service" => AddPriceFluctuation(modelData.EnergyPerKwH, usageRates["Small Industrial"], multiplier),
                "Average NGO" => AddPriceFluctuation(modelData.EnergyPerKwH, usageRates["Average NGO"], multiplier),
                "Average Household" => AddPriceFluctuation(modelData.EnergyPerKwH, usageRates["Average Household"], multiplier),
                "Average Business" => AddPriceFluctuation(modelData.EnergyPerKwH, usageRates["Average Business"], multiplier * 0.15),
                "Average Industrial" => AddPriceFluctuation(modelData.EnergyPerKwH, usageRates["Average Industrial"], multiplier * 0.20),
                "Average Public Service" => AddPriceFluctuation(modelData.EnergyPerKwH, usageRates["Average Industrial"], multiplier),
                "Large NGO" => AddPriceFluctuation(modelData.EnergyPerKwH, usageRates["Large NGO"], multiplier),
                "Large Household" => AddPriceFluctuation(modelData.EnergyPerKwH, usageRates["Large Household"], multiplier),
                "Large Business" => AddPriceFluctuation(modelData.EnergyPerKwH, usageRates["Large Business"], multiplier * 0.20),
                "Large Industrial" => AddPriceFluctuation(modelData.EnergyPerKwH, usageRates["Large Industrial"], multiplier * 0.25),
                "Large Public Service" => AddPriceFluctuation(modelData.EnergyPerKwH, usageRates["Large Industrial"], multiplier),
                _ => 0
            };
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