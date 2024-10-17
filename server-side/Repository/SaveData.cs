using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using server_side.Repository.Interface;

namespace server_side.Repository;

public class SaveData : ISaveData
{
    private readonly IWattWiseFolderPath _wattWiseFolderPath;
    public SaveData(IWattWiseFolderPath wattWiseFolderPath)
    {
        _wattWiseFolderPath = wattWiseFolderPath;
    }

       public bool ListToJson(UserData usersData)
        {
            try
            {
                string jsonFilePath = Path.Combine(_wattWiseFolderPath.GetWattWiseFolderPath(), "server-side", "Data", "UserJson.json");

               // var getDirectory = Environment.CurrentDirectory;
              //  var filePath = getDirectory + "\\server-side\\Data\\UserJson.json";
                string existingJson = File.ReadAllText(jsonFilePath);
                JArray userInfo = JArray.Parse(existingJson);
                
                var userToUpdate = userInfo.FirstOrDefault(u => (int)u["UserID"] == usersData.UserID);
                if (userToUpdate != null)
                {
                    userInfo.Remove(userToUpdate);
                }
                
                
                JObject userDataObject = new JObject
                {
                    { "Address", usersData.Address},
                    { "UserID", usersData.UserID},
                    { "firstName", usersData.firstName},
                    { "lastName", usersData.lastName},
                    { "UserEmail", usersData.UserEmail},
                    { "Passcode", usersData.Passcode},
                    {"Hash" , usersData.Hash},
                    { "SmartDevice", new JObject {
                            { "SmartMeterID", usersData.SmartMeterID},
                            { "EnergyPerKwH", usersData.EnergyPerKwH },
                            {"CurrentMonthCost", usersData.CurrentMonthCost}
                         }
                    }
                };
                
                userInfo.Add(userDataObject);
                
                string updatedJson = JsonConvert.SerializeObject(userInfo, Formatting.Indented);
                File.WriteAllText(jsonFilePath, updatedJson);

                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("Error Writing to File: " + e.ToString());
                return false;
            }
        }
}