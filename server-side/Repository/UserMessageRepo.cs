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
            
            return usersList.FirstOrDefault(user => user.UserID == UserID);
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
                    SmartMeterData = userData.SmartMeterData
                }
            };
            string serializedUserData = JsonConvert.SerializeObject(users);
            var hashedUserdData = Cryptography.Cryptography.GenerateHash(serializedUserData);
            users.Hash = hashedUserdData;
            var result = ListToJson(users);
            return result;
        }

        public bool ListToJson(UserData users)
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

                JObject userDataObject = new JObject
                {
                    { "Address", users.Address},
                    { "UserID", users.UserID},
                    { "firstName", users.firstName},
                    { "lastName", users.lastName},
                    { "UserEmail", users.UserEmail},
                    { "Passcode", users.Passcode},
                    {"Hash" , users.Hash},
                    { "SmartDevice", new JObject {
                            { "SmartMeterID", users.SmartMeterID},
                            { "SmartMeterData", users.SmartMeterData }
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
