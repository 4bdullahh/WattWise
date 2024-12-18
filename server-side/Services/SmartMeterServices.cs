﻿using server_side.Services.Interface;
using server_side.Repository.Interface;
using Newtonsoft.Json;
using server_side.Repository;
using server_side.Repository.Models;
using server_side.Services.Models;

namespace server_side.Services
{
    /*
     * Class Documentation:
        This is the smart meter service which is connected with 
        the repository to handle smart meter enquiries
     */
    public class SmartMeterServices: ISmartMeterServices
    {
        private readonly ISmartMeterRepo _smartMeterRepo;
        private readonly string _decryptedMessage;
        private readonly IErrorLogRepo _errorLogRepo;
        private readonly ErrorLogMessage _errorLogMessage;
        public SmartMeterServices(ISmartMeterRepo smartMeterRepo, IErrorLogRepo errorLogRepo)
        {
             _smartMeterRepo = smartMeterRepo;
             _errorLogRepo = errorLogRepo;
             _errorLogMessage = new ErrorLogMessage();
        }

        public SmartMeterResponse UpdateMeterServices(string decryptedMessage)
        {
            
            try
            {
                SmartDevice smartDevice = JsonConvert.DeserializeObject<SmartDevice>(decryptedMessage);
                _errorLogMessage.ClientId = smartDevice.SmartMeterId;

                var meterReadings = _smartMeterRepo.UpdateMeterData(smartDevice);

                var userMessageRepo = new UserMessageRepo(
                    saveData: new SaveData(),              
                    smartMeterRepo: _smartMeterRepo,
                    errorLogRepo: _errorLogRepo
                );
                if (meterReadings.Message.Contains("Power grid outage"))
                {
                    Console.WriteLine("Power grid outage...");
                    return new SmartMeterResponse
                    {
                        SmartMeterID = meterReadings.SmartMeterId,
                        EnergyPerKwH = meterReadings.EnergyPerKwH,
                        CurrentMonthCost = meterReadings.CurrentMonthCost,
                        KwhUsed = meterReadings.KwhUsed,
                        Message = meterReadings.Message
                    };
                }
                else if (meterReadings.Message.Contains("Cost calculation"))
                {
                    var userToUpdate = userMessageRepo.GetById(meterReadings.UserData.UserID);
                    userToUpdate.SmartMeterId = smartDevice.SmartMeterId;
                    userMessageRepo.UpdateUserData(userToUpdate);
                    
                    return new SmartMeterResponse
                    {
                        SmartMeterID = meterReadings.SmartMeterId,
                        EnergyPerKwH = meterReadings.EnergyPerKwH,
                        CurrentMonthCost = meterReadings.CurrentMonthCost,
                        KwhUsed = meterReadings.KwhUsed,
                        Message = $"Current Month Cost {meterReadings.CurrentMonthCost}"
                    };
                }
                else
                {
                    return new SmartMeterResponse
                    {
                        Message = "SmartMeter not found"
                    };
                }
                
                
            }
            catch (Exception e)
            {
                _errorLogMessage.Message = $"Server: ClientID {_errorLogMessage.ClientId} Unable to access smart meter repo from SmartMeterServices: {e.Message} : {DateTime.UtcNow}";
                Console.WriteLine($"{_errorLogMessage.Message} {e.Message}");
                _errorLogRepo.LogError(_errorLogMessage);
                throw;
            }

        }
        
    }
}

