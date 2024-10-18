using System.ServiceModel.Channels;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using server_side.Repository.Interface;
using server_side.Services.Models;

namespace server_side.Repository;

public class SmartMeterRepo : ISmartMeterRepo
{
    private List<SmartDevice> meterList;
    private readonly IWattWiseFolderPath _wattWiseFolderPath;
    private readonly ISaveData _saveData;
    public SmartMeterRepo(IWattWiseFolderPath wattWiseFolderPath, ISaveData saveData)
    {
        _wattWiseFolderPath = wattWiseFolderPath;
        _saveData = saveData;
        LoadUserData();
    }
    
    private void LoadUserData()
    {
        string jsonFilePath = Path.Combine(_wattWiseFolderPath.GetWattWiseFolderPath(), "server-side", "Data", "MeterJson.json");
            
        if (File.Exists(jsonFilePath))
        {
            string jsonData = File.ReadAllText(jsonFilePath);
            JArray _users = JArray.Parse(jsonData);
            meterList = JsonConvert.DeserializeObject<List<SmartDevice>>(_users.ToString());
        }
        else
        {
            File.Create(jsonFilePath).Close();
            LoadUserData();
        }
    }

    public SmartDevice GetById(int SmartMeterID)
    {
        var smartMeterById = meterList.FirstOrDefault(x => x.SmartMeterID == SmartMeterID);
        if (smartMeterById != null)
        {
            return smartMeterById;
        }
        else
        {
            return new SmartDevice
            {
                Message = "Smart Device Not Found"
            };
        }
    }
    public SmartDevice UpdateMeterRepo(SmartDevice smartDevice)
    {
       var existingDevice = meterList.FirstOrDefault(x => x.SmartMeterID == smartDevice.SmartMeterID);

       if (existingDevice != null)
       {
           existingDevice.SmartMeterID = smartDevice.SmartMeterID;
           existingDevice.EnergyPerKwH = smartDevice.EnergyPerKwH;
           existingDevice.CurrentMonthCost = smartDevice.CurrentMonthCost;
           
           var result = _saveData.ListToJson(existingDevice);
           return result;
       }
       else
       {
           AddMeterData(existingDevice);
           return new SmartDevice
           {
               SmartMeterID = smartDevice.SmartMeterID,
               EnergyPerKwH = smartDevice.EnergyPerKwH,
               CurrentMonthCost = smartDevice.CurrentMonthCost,
               Message = "Smart Device Does Not Exist. New User Added"
           };
       }
       
    }
    
    public bool AddMeterData(SmartDevice smartDevice)
    {
        var smartMeters = new SmartDevice
        {
            SmartMeterID = smartDevice.SmartMeterID,
            EnergyPerKwH = smartDevice.EnergyPerKwH,
            CurrentMonthCost = smartDevice.CurrentMonthCost
        };
        
        var result = _saveData.ListToJson(smartMeters);
        return result;
    }

}