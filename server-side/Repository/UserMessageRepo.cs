
using server_side.Repository.Interface;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace server_side.Repository
{
    public class UserMessageRepo : IUserMessageRepo
    {
        private List<UserData> usersList;
        private IHashHandle userHash;
        private readonly IWattWiseFolderPath _wattWiseFolderPath;
        private readonly ISaveData _saveData;
        public UserMessageRepo(IWattWiseFolderPath wattWiseFolderPath, ISaveData saveData)
        {
            _wattWiseFolderPath = wattWiseFolderPath;
            _saveData = saveData;
            LoadUserData();
        }

        private void LoadUserData()
        {
            string jsonFilePath = Path.Combine(_wattWiseFolderPath.GetWattWiseFolderPath(), "server-side", "Data", "UserJson.json");
            
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

        public UserData GetById(int UserID)
        {
            var user = usersList.FirstOrDefault(user => user.UserID == UserID);

            if (user != null)
            {
                return user;
            }
            else
            {
                return null;
            }
        }

        public bool UpdateUserData(UserData user)
        {
            var existingUser = usersList.FirstOrDefault(u => u.UserID == user.UserID);
            
            existingUser.UserID =  user.UserID;
            existingUser.firstName = user.firstName;
            existingUser.lastName = user.lastName;
            existingUser.Address = user.Address;
            existingUser.UserEmail = user.UserEmail;
            existingUser.Passcode = user.Passcode;
            existingUser.SmartMeterID = user.SmartMeterID;
            existingUser.EnergyPerKwH = user.EnergyPerKwH;
            existingUser.CurrentMonthCost = user.CurrentMonthCost;
            
            string serializedUserData = JsonConvert.SerializeObject(existingUser);
            var hashedUserdData = Cryptography.Cryptography.GenerateHash(serializedUserData);
            existingUser.Hash = hashedUserdData;
            var result = _saveData.ListToJson(existingUser);
            return result;
        }
        
        public bool AddUserData(UserData userData)
        {
            var users = new UserData
            {
                UserID = userData.UserID,
                firstName = userData.firstName,
                lastName = userData.lastName,
                Address = userData.Address,
                UserEmail = userData.UserEmail,
                Passcode = userData.Passcode,
                SmartDevice = new SmartDevice
                {
                    SmartMeterID = userData.SmartMeterID,
                    EnergyPerKwH = userData.EnergyPerKwH,
                    CurrentMonthCost = userData.CurrentMonthCost
                }
            };
            string serializedUserData = JsonConvert.SerializeObject(users);
            var hashedUserdData = Cryptography.Cryptography.GenerateHash(serializedUserData);
            users.Hash = hashedUserdData;
            var result = _saveData.ListToJson(users);
            return result;
        }

 
   
    }
}

