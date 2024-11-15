using System.Diagnostics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using server_side.Repository.Interface;
using server_side.Repository.Models;
using server_side.Services;

namespace server_side.Repository;

public class PowerGridCalc :IPowerGridCalc
{
    private List<SmartDevice> _smartDevices;
    private readonly ErrorLogMessage _errorLogMessage;
    private PowerGridTracker _powerGridModel;
    private readonly IErrorLogRepo _errorLogRepo;
    private FolderPathServices folderpath;

    /*
     * Class Documentation:
        This class is responsible to generate power grid simulation
        and have methods to calculate and generate power grid outage
     */
    public PowerGridCalc(IErrorLogRepo errorLogRepo)
    {
        _errorLogMessage = new ErrorLogMessage();
        folderpath = new FolderPathServices();
        _errorLogRepo = errorLogRepo;
        _powerGridModel = new PowerGridTracker();
        _powerGridModel.clientList = new List<int>();
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
                JArray smartDevices = JArray.Parse(jsonData);
                _smartDevices = JsonConvert.DeserializeObject<List<SmartDevice>>(smartDevices.ToString());
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
        return _smartDevices;
    }

    public SmartDevice CalculateGridOutage(SmartDevice smartDevice)
    {
        var random = new Random();
        double multiplier = 0.1 + random.NextDouble() * 0.1;
        double kwhCutOff = 30;

        if (!_powerGridModel.clientList.Contains(smartDevice.SmartMeterId))
        {
            for (int i = 0; i < _smartDevices.Count; i++)
            {
                if (smartDevice.SmartMeterId == _smartDevices[i].SmartMeterId)
                {
                    _powerGridModel.clientList.Add(smartDevice.SmartMeterId);
                    _powerGridModel.kwhLimit += _smartDevices[i].EnergyPerKwH + multiplier;
                    
                }
            }
        }
       
        if (_powerGridModel.kwhLimit >= kwhCutOff)
        {
            smartDevice.Message = $"Power grid outage usage at {_powerGridModel.kwhLimit} kwh : {DateTime.Now}" ;
            smartDevice.clientList = _powerGridModel.clientList;
            _errorLogMessage.Message = smartDevice.Message;
            _errorLogRepo.LogError(_errorLogMessage);
            Console.WriteLine(smartDevice.Message);
            _powerGridModel.clientList.Clear();
            _powerGridModel.kwhLimit = 0;
            return smartDevice;
        }
        
        return null;

    }
}