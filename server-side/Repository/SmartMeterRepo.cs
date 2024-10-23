using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using server_side.Repository.Interface;
using server_side.Services;

namespace server_side.Repository;

public class SmartMeterRepo : ISmartMeterRepo
{
    private List<SmartDevice> meterList;
    private FolderPathServices folderpath;
    private readonly ISaveData _saveData;
    public SmartMeterRepo(ISaveData saveData)
    {
        folderpath = new FolderPathServices();
        _saveData = saveData;
        LoadUserData();
    }
    
    private void LoadUserData()
    {
        string jsonFilePath = Path.Combine(folderpath.GetWattWiseFolderPath(), "server-side", "Data", "MeterJson.json");
            
        if (File.Exists(jsonFilePath))
        {
            string jsonData = File.ReadAllText(jsonFilePath);
            JArray _smartMeters = JArray.Parse(jsonData);
            meterList = JsonConvert.DeserializeObject<List<SmartDevice>>(_smartMeters.ToString());
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
            
            var device = new SmartDevice();
            AddMeterData(device);
           
            return device;
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

           var device = new SmartDevice();
           AddMeterData(device);
           
           return device;
       }
       
    }
    
    public void AddMeterData(SmartDevice smartDevice)
    {
        var generateId = meterList.Count();
        
        smartDevice.SmartMeterID = generateId;
        smartDevice.EnergyPerKwH = 0;
        smartDevice.CurrentMonthCost = 0;
        
        
         _saveData.ListToJson(smartDevice);
    }

}