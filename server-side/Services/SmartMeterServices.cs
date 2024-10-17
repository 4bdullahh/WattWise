using server_side.Services.Interface;
using server_side.Repository.Interface;
using NetMQ;
using NetMQ.Sockets;
using Newtonsoft.Json;
using System.Text;
using DotNetEnv;
using server_side.Cryptography;
namespace server_side.Services
{
    public class SmartMeterServices: ISmartMeterServices
    {
        private readonly ISmartMeterRepo _smartMeterRepo;
        private readonly string _rsaPrivateKey;
        private readonly IFolderPathServices _folderPathServices;
        
        public SmartMeterServices(ISmartMeterRepo smartMeterRepo, IFolderPathServices folderPathServices)
        {
            _smartMeterRepo = smartMeterRepo;
            _folderPathServices = folderPathServices;
            string serverSideFolderPath = _folderPathServices.GetServerSideFolderPath();
            var envGenerator = new GenerateEnvFile();
            envGenerator.EnvFileGenerator();
            Env.Load(serverSideFolderPath + "\\.env");
            _rsaPrivateKey = Env.GetString("RSA_PRIVATE_KEY");
        }
        
         
    
    }
}

