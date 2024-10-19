﻿using Newtonsoft.Json;
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
    
       public T ListToJson<T>(T data)
        {
            try
            {

                string filePath = ""; 
                filePath = data is UserData ? "UserJson" : "MeterJson";
                
                string jsonFilePath = Path.Combine(_wattWiseFolderPath.GetWattWiseFolderPath(), "server-side", "Data", $"{filePath}.json");
                
                string existingJson = File.ReadAllText(jsonFilePath);
                JArray userInfo = JArray.Parse(existingJson);


                if (data is UserData userData)
                {
                    var userToUpdate = userInfo.FirstOrDefault(u => (int)u["UserID"] == userData.UserID);
                    if (userToUpdate != null)
                    {
                        userInfo.Remove(userToUpdate);
                    }
                    
                    JObject userDataObject = new JObject
                    {
                        { "Address", userData.Address},
                        { "UserID", userData.UserID},
                        { "firstName", userData.firstName},
                        { "lastName", userData.lastName},
                        { "UserEmail", userData.UserEmail},
                        { "Passcode", userData.Passcode},
                        {"Hash" , userData.Hash},
                        { "SmartDevice", new JObject {
                                { "SmartMeterID", userData.SmartMeterID},
                                { "EnergyPerKwH", userData.EnergyPerKwH },
                                {"CurrentMonthCost", userData.CurrentMonthCost}
                            }
                        }
                    };
                    
                    userInfo.Add(userDataObject);
                }
                else if (data is SmartDevice smartDevice)
                {
                    var deviceToUpdate = userInfo.FirstOrDefault(u => (int)u["SmartMeterID"] == smartDevice.SmartMeterID);
                    if (deviceToUpdate != null)
                    {
                        userInfo.Remove(deviceToUpdate);
                    }

                    JObject smartDataObject = new JObject
                    {
                        { "SmartMeterID", smartDevice.SmartMeterID },
                        { "EnergyPerKwH", smartDevice.EnergyPerKwH },
                        { "CurrentMonthCost", smartDevice.CurrentMonthCost }
                    };
                    userInfo.Add(smartDataObject);
                }
                else
                {
                    throw new ArgumentException("Unsupported data type");
                }
                
                string updatedJson = JsonConvert.SerializeObject(userInfo, Formatting.Indented);
                File.WriteAllText(jsonFilePath, updatedJson);

                return data;
            }
            catch (Exception e)
            {
                Console.WriteLine("Error Writing to File: " + e.ToString());
                return data;
            }
        }
    
}