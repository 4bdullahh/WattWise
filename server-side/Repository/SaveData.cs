﻿using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using server_side.Repository.Interface;
using server_side.Repository.Models;
using server_side.Services;

namespace server_side.Repository;

public class SaveData : ISaveData
{
    
    /*
     * Class Documentation:
        This class is responsible the mock database that store data on Json file.
     */
    private FolderPathServices folderpath;
    private IErrorLogRepo errorLogRepo;
    private ErrorLogMessage errorLogMessage;
    public SaveData()
    {
        folderpath= new FolderPathServices();
        errorLogMessage = new ErrorLogMessage();
        
    }
    
       public T ListToJson<T>(T data)
        {
            try
            {

               string filePath = data is UserData ? "UserJson" : "MeterJson";
                
                string jsonFilePath = Path.Combine(folderpath.GetWattWiseFolderPath(), "server-side", "Data", $"{filePath}.json");
                
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
                        { "SmartDevice", new JObject {
                                { "SmartMeterID", userData.SmartMeterId},
                                { "EnergyPerKwH", userData.EnergyPerKwH },
                                {"CurrentMonthCost", userData.CurrentMonthCost},
                                {"CustomerType", userData.CustomerType}
                            }
                        }
                    };

                    userInfo.Add(userDataObject);
                }
                else if (data is SmartDevice smartDevice)
                {
                    var deviceToUpdate =
                        userInfo.FirstOrDefault(u => (int)u["SmartMeterID"] == smartDevice.SmartMeterId);
                    if (deviceToUpdate != null)
                    {
                        deviceToUpdate["CurrentMonthCost"] = smartDevice.CurrentMonthCost;
                        deviceToUpdate["EnergyPerKwH"] = smartDevice.EnergyPerKwH;
                        deviceToUpdate["KwhUsed"] = smartDevice.KwhUsed;
                    }
                    else
                    {

                        JObject smartDataObject = new JObject
                        {
                            { "SmartMeterID", smartDevice.SmartMeterId },
                            { "EnergyPerKwH", smartDevice.EnergyPerKwH },
                            { "CurrentMonthCost", smartDevice.CurrentMonthCost },
                            { "KwdUsed", smartDevice.KwhUsed },
                            { "CustomerType", smartDevice.CustomerType }
                        };
                        userInfo.Add(smartDataObject);
                    }
                }
                else
                {
                    throw new ArgumentException("Unsupported data type");
                }

                string updatedJson = JsonConvert.SerializeObject(userInfo, Formatting.Indented);
                File.WriteAllText(jsonFilePath, updatedJson);

                return data;
            }
            catch (ArgumentException)
            {
                throw new ArgumentException("Unsupported data type");
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error Writing to File: {e.Message}");
                return data;
            }
        }
    
}