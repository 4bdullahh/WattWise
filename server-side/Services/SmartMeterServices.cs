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
            SmartDevice smartDevice = JsonConvert.DeserializeObject<SmartDevice>(decryptedMessage);
            
            var meterReadings = _smartMeterRepo.UpdateMeterRepo(smartDevice);
            return meterReadings;
        }
         
    
    }
}

