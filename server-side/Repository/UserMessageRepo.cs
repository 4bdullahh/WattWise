
using System.ComponentModel;
using server_side.Repository.Interface;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using server_side.Repository.Models;
using server_side.Services;

namespace server_side.Repository
{
    public class UserMessageRepo : IUserMessageRepo
    {
        private List<UserData> usersList;
        private IHashHandle userHash;
        private FolderPathServices folderpath;
        private readonly ISaveData _saveData;
        private readonly ISmartMeterRepo _smartMeterRepo;
        private ErrorLogMessage _errorLogMessage;
        private IErrorLogRepo _errorLogRepo;
        public UserMessageRepo(ISaveData saveData, ISmartMeterRepo smartMeterRepo, IErrorLogRepo errorLogRepo)
        {
            folderpath= new FolderPathServices();
            _saveData = saveData;
            _smartMeterRepo = smartMeterRepo;
            _errorLogRepo = errorLogRepo;
            _errorLogMessage = new ErrorLogMessage();
            LoadUserData();
            
        }

        
        private void LoadUserData()
        {
            string jsonFilePath = Path.Combine(folderpath.GetWattWiseFolderPath(), "server-side", "Data", "UserJson.json");
            
            try
            {
                if (File.Exists(jsonFilePath))
                {
                    string jsonData = File.ReadAllText(jsonFilePath);
                    JArray _users = JArray.Parse(jsonData);
                    usersList = JsonConvert.DeserializeObject<List<UserData>>(_users.ToString());
                }
                else
                {
                    File.Create(jsonFilePath).Close();
                    LoadUserData();
                }
            }
            catch (Exception e)
            {
                _errorLogMessage.Message = $"Server: ClientID {_errorLogMessage.ClientId} We could not load users data: {e.Message} : {DateTime.UtcNow}";
                Console.WriteLine($"{_errorLogMessage.Message} {e.Message}");
                _errorLogRepo.LogError(_errorLogMessage);
                throw;
            }
        }

        public UserData GetById(int UserID)
        {
            var user = usersList.FirstOrDefault(user => user.UserID == UserID);

            try
            {
                //throw new Exception("Intentional failure");
                if (user != null)
                {
                    return user;
                }
                
                return null;
                
            }
            catch (Exception e)
            {
                _errorLogMessage.Message = $"Server: ClientID {_errorLogMessage.ClientId} UserMessageRepo could not fetch user ID: {e.Message} : {DateTime.UtcNow}";
                Console.WriteLine($"{_errorLogMessage.Message} {e.Message}");
                _errorLogRepo.LogError(_errorLogMessage);
                throw;
            }
        }

        public UserData UpdateUserData(UserData user)
        {
            try
            {
                var existingUser = usersList.FirstOrDefault(u => u.UserID == user.UserID);
                var getSmartMeter = _smartMeterRepo.GetById(user.SmartMeterId);
                
                if (existingUser == null || getSmartMeter == null)
                {
                    throw new NullReferenceException($"The existingUser {existingUser.UserID} or SmartMeter {getSmartMeter.SmartMeterId} has returned null.");
                }

                existingUser.UserID = user.UserID;
                existingUser.SmartMeterId = getSmartMeter.SmartMeterId;
                existingUser.EnergyPerKwH = Math.Round(getSmartMeter.EnergyPerKwH, 2);
                existingUser.CurrentMonthCost = Math.Round(getSmartMeter.CurrentMonthCost, 2);
                existingUser.CustomerType = getSmartMeter.CustomerType;

                string serializedUserData = JsonConvert.SerializeObject(existingUser);
                var hashedUserdData = Cryptography.Cryptography.GenerateHash(serializedUserData);
                existingUser.Hash = hashedUserdData;
                var result = _saveData.ListToJson(existingUser);
                if (result == null)
                {
                    throw new NullReferenceException($"The result of ListToJson {result} has returned null.");
                }
                
                return result;
            }
            catch (NullReferenceException e)
            {
                _errorLogMessage.Message = $"Server: From UserID {user.UserID} in UserMessageRepo: {e.Message} : {DateTime.UtcNow}";
                Console.WriteLine(_errorLogMessage.Message);
                _errorLogRepo.LogError(_errorLogMessage);
                throw;
            }
            catch (Exception e)
            {
                _errorLogMessage.Message = $"Server: ClientID {_errorLogMessage.ClientId} UserMessageRepo could not update user data: {e.Message} : {DateTime.UtcNow}";
                Console.WriteLine($"{_errorLogMessage.Message} {e.Message}");
                _errorLogRepo.LogError(_errorLogMessage);
                throw;
            }
        }
        
        public UserData AddUserData(UserData userData)
        {
            try
            {
                var generateId = usersList.Count();
            
                var smartMeter = _smartMeterRepo.GetById(userData.SmartMeterId);
            
                var users = new UserData
                {
                    UserID = generateId,
                    firstName = userData.firstName,
                    lastName = userData.lastName,
                    Address = userData.Address,
                    UserEmail = userData.UserEmail,
                    SmartDevice = new SmartDevice
                    {
                        SmartMeterId = smartMeter.SmartMeterId,
                        EnergyPerKwH = smartMeter.EnergyPerKwH,
                        CurrentMonthCost = smartMeter.CurrentMonthCost
                    }
                };
                string serializedUserData = JsonConvert.SerializeObject(users);
                var hashedUserdData = Cryptography.Cryptography.GenerateHash(serializedUserData);
                users.Hash = hashedUserdData;
                var result = _saveData.ListToJson(users);
                return result;
                
            }
            catch (Exception e)
            {
                _errorLogMessage.Message = $"Server: ClientID {_errorLogMessage.ClientId} UserMessageRepo could not add user data: {e.Message} : {DateTime.UtcNow}";
                Console.WriteLine($"{_errorLogMessage.Message} {e.Message}");
                _errorLogRepo.LogError(_errorLogMessage);
                throw;
            }
        }
    }
}
