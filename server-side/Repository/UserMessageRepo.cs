
using server_side.Repository.Interface;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
        public UserMessageRepo(ISaveData saveData, ISmartMeterRepo smartMeterRepo)
        {
            folderpath= new FolderPathServices();
            _saveData = saveData;
            _smartMeterRepo = smartMeterRepo;
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
                Console.WriteLine($"We could not load users data: {e.Message}");
                throw;
            }
        }

        public UserData GetById(int UserID)
        {
            var user = usersList.FirstOrDefault(user => user.UserID == UserID);

            try
            {
                if (user != null)
                {
                    return user;
                }

                return null;
            }
            catch (Exception e)
            {
                Console.WriteLine($"We could not get user id: {e.Message}");
                throw;
            }
        }

        public UserData UpdateUserData(UserData user)
        {
            try
            {
                var existingUser = usersList.FirstOrDefault(u => u.UserID == user.UserID);

                existingUser.UserID = user.UserID;
                existingUser.firstName = user.firstName;
                existingUser.lastName = user.lastName;
                existingUser.Address = user.Address;
                existingUser.UserEmail = user.UserEmail;
                existingUser.SmartMeterId = user.SmartMeterId;
                existingUser.EnergyPerKwH = user.EnergyPerKwH;
                existingUser.CurrentMonthCost = user.CurrentMonthCost;
                existingUser.CustomerType = user.CustomerType;

                string serializedUserData = JsonConvert.SerializeObject(existingUser);
                var hashedUserdData = Cryptography.Cryptography.GenerateHash(serializedUserData);
                existingUser.Hash = hashedUserdData;
                var result = _saveData.ListToJson(existingUser);
                return result;
            }
            catch (Exception e)
            {
                Console.WriteLine($"We could not update user data: {e.Message}");
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
                    Passcode = userData.Passcode,
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
                Console.WriteLine($"We could not add user data: {e.Message}");
                throw;
            }
        }
    }
}
