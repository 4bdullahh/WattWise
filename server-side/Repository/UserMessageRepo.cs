using System.Text;
using server_side.Repository.Interface;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace server_side.Repository
{
    public class UserMessageRepo : IUserMessageRepo
    {
        private List<UserData> usersList;
        private IHashHandle userHash;
        public UserMessageRepo()
        {
            LoadUserData();
        }

        private void LoadUserData()
        {
            string solutionFolderName = "WattWise";
            var currentDirectory = new DirectoryInfo(Environment.CurrentDirectory);
            
            while (currentDirectory != null && currentDirectory.Name != solutionFolderName)
            {
                currentDirectory = currentDirectory.Parent;
            }

            if (currentDirectory == null)
            {
                throw new DirectoryNotFoundException("Could not find the current directory");
            }
            
            string jsonFilePath = Path.Combine(currentDirectory.FullName, "server-side", "Data", "UserJson.json");

            
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
            var result = ListToJson(existingUser);
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
            var result = ListToJson(users);
            return result;
        }

        public bool ListToJson(UserData usersData)
        {
            try
            {
                string solutionFolderName = "WattWise";
                var currentDirectory = new DirectoryInfo(Environment.CurrentDirectory);
            
                while (currentDirectory != null && currentDirectory.Name != solutionFolderName)
                {
                    currentDirectory = currentDirectory.Parent;
                }

                if (currentDirectory == null)
                {
                    throw new DirectoryNotFoundException("Could not find the current directory");
                }
            
                string jsonFilePath = Path.Combine(currentDirectory.FullName, "server-side", "Data", "UserJson.json");

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
}
