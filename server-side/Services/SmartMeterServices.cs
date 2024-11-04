using server_side.Services.Interface;
using server_side.Repository.Interface;
using NetMQ;
using NetMQ.Sockets;
using Newtonsoft.Json;
using System.Text;
using DotNetEnv;
using server_side.Cryptography;
using server_side.Services.Models;

namespace server_side.Services
{
    public class SmartMeterServices: ISmartMeterServices
    {
        private readonly ISmartMeterRepo _smartMeterRepo;
        private readonly string _decryptedMessage;
        
        
        public SmartMeterServices(ISmartMeterRepo smartMeterRepo)
        {
             _smartMeterRepo = smartMeterRepo;
        }

        public SmartMeterResponse UpdateMeterServices(string decryptedMessage)
        { 
            try
            {
                SmartDevice smartDevice = JsonConvert.DeserializeObject<SmartDevice>(decryptedMessage);

                var meterReadings = _smartMeterRepo.UpdateMeterRepo(smartDevice);

                if (meterReadings != null)
                {
                    return new SmartMeterResponse
                    {
                        SmartMeterID = meterReadings.SmartMeterID,
                        EnergyPerKwH = meterReadings.EnergyPerKwH,
                        CurrentMonthCost = meterReadings.CurrentMonthCost,
                        Message = $"Current Month Cost {meterReadings.CurrentMonthCost}"
                    };
                }

                return new SmartMeterResponse
                {
                    Message = "SmartMeter not found"
                };
            }
            catch (Exception e)
            {
                Console.WriteLine($"We were unable to process the message: {e.Message}");
                throw;
            }

        }
        
    }
}

