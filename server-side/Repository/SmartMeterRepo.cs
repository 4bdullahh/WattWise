using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using server_side.Repository.Interface;
using server_side.Repository.Models;
using server_side.Services;

namespace server_side.Repository;

public class SmartMeterRepo : ISmartMeterRepo
{
    private List<SmartDevice> meterList;
    private FolderPathServices folderpath;
    private readonly ISaveData _saveData;
    private readonly ICalculateCost _calculateCost;
    private readonly IErrorLogRepo _errorLogRepo;
    private readonly ErrorLogMessage _errorLogMessage;
    
    public SmartMeterRepo(ISaveData saveData, ICalculateCost calculateCost, IErrorLogRepo errorLogRepo)
    {
        folderpath = new FolderPathServices();
        _saveData = saveData;
        _calculateCost = calculateCost;
        _errorLogRepo = errorLogRepo;
        _errorLogMessage = new ErrorLogMessage();
        LoadSmartMeterData();
    }
    
    public List<SmartDevice> LoadSmartMeterData()
    {
        string jsonFilePath = Path.Combine(folderpath.GetWattWiseFolderPath(), "server-side", "Data", "MeterJson.json");
            
        try
        {
            if (File.Exists(jsonFilePath))
            {
                string jsonData = File.ReadAllText(jsonFilePath);
                JArray _smartMeters = JArray.Parse(jsonData);
                meterList = JsonConvert.DeserializeObject<List<SmartDevice>>(_smartMeters.ToString());
            }
            else
            {
                File.Create(jsonFilePath).Close();
                LoadSmartMeterData();
            }
        }
        catch (Exception e)
        {
            _errorLogMessage.Message = $"Server: ClientID {_errorLogMessage.ClientId} SmartMeterRepo could not load user data: {e.Message} : {DateTime.UtcNow}";
            Console.WriteLine($"{_errorLogMessage.Message}");
            _errorLogRepo.LogError(_errorLogMessage);
            throw;
        }
        return meterList;
    }

    public SmartDevice GetById(int SmartMeterID)
    {
        try
        {
            //throw new Exception("Intentional error");
            var smartMeterById = meterList.FirstOrDefault(x => x.SmartMeterId == SmartMeterID);

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
        catch (Exception e)
        {
            _errorLogMessage.Message = $"Server: ClientID {_errorLogMessage.ClientId} SmartMeterRepo could not get user id: {e.Message} : {DateTime.UtcNow}";
            Console.WriteLine($"{_errorLogMessage.Message}");
            _errorLogRepo.LogError(_errorLogMessage);
            throw;
        }
    }
    public SmartDevice UpdateMeterData(SmartDevice smartDevice)
     {

       try
       {
          var existingDevice = meterList.FirstOrDefault(x => x.SmartMeterId == smartDevice.SmartMeterId);

          if (existingDevice != null)
          {
              var calculateReadings = _calculateCost.getCurrentBill(smartDevice);
              
              
              existingDevice.SmartMeterId = calculateReadings.SmartMeterId;
              existingDevice.EnergyPerKwH = Math.Round(calculateReadings.EnergyPerKwH,2);
              existingDevice.CurrentMonthCost = calculateReadings.CurrentMonthCost;
              existingDevice.KwhUsed = Math.Round(calculateReadings.KwhUsed, 2);
              var result = _saveData.ListToJson(existingDevice);
              return result;
          }
          {
   
              var device = new SmartDevice();
              AddMeterData(device);
              return device;
          }
       } 
    
       catch (Exception e)
       {
           _errorLogMessage.Message = $"Server: ClientID {_errorLogMessage.ClientId} SmartMeterRepo could not update device: {e.Message} : {DateTime.UtcNow}";
           Console.WriteLine($"{_errorLogMessage.Message}");
           _errorLogRepo.LogError(_errorLogMessage);
           throw;
       }
    }
    
    public void AddMeterData(SmartDevice smartDevice)
    {
        try
        {
             var generateId = meterList.Count();
                    
                    smartDevice.SmartMeterId = generateId;
                    smartDevice.EnergyPerKwH = 0;
                    smartDevice.CurrentMonthCost = 0;
                    
                    
                     _saveData.ListToJson(smartDevice);
        } catch (Exception e)
        {
            _errorLogMessage.Message = $"Server: ClientID {_errorLogMessage.ClientId} SmartMeterRepo could not add device: {e.Message} : {DateTime.UtcNow}";
            Console.WriteLine($"{_errorLogMessage.Message}");
            _errorLogRepo.LogError(_errorLogMessage);
            throw;
        }
       
    }

}