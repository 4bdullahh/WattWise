using server_side.Repository.Interface;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;
using Newtonsoft.Json;

namespace server_side.Repository
{
    public class UserMessageRepo : IUserMessageRepo
    {
        private readonly List<UserData> userDatabase;
        private List<UserData> usersList;
        public UserMessageRepo()
        {
            userDatabase = new List<UserData>();
            LoadUserData();
        }
        
        private void LoadUserData()
        {
            // Path to your JSON file
            var jsonFilePath = "C:/Users/Muhammad.Mamoon/Documents/ENTERPRISE DESIGN/WattWise/server-side/Data/UserJson.json";

            if (File.Exists(jsonFilePath))
            {
                string jsonData = File.ReadAllText(jsonFilePath);
                JArray _users = JArray.Parse(jsonData);
                usersList = JsonConvert.DeserializeObject<List<UserData>>(_users.ToString());
            }
            else
            {
                usersList = new List<UserData>();
            }
        }

        public UserData GetById(int UserID)
        {
            return usersList.FirstOrDefault(user => user.UserID == UserID);
        }


        public bool AddUserData(UserData userData)
        {
            var result = ListToJson(userData);
            return result;
        }

        public UserData UpdateData(UserData userData)
        {

            var existingMessage = GetById(userData.UserID);
            if (existingMessage != null)
            {
                userDatabase.Remove(existingMessage);
                userDatabase.Add(userData);
                return userData;
            }
            else
            {
                return userData;
            }
        }


        //WRITE SAVE TO FILE METHOD USE 
        public bool ListToJson(UserData users)
        {
            try{
                string filePath = "C:/Users/Muhammad.Mamoon/Documents/ENTERPRISE DESIGN/WattWise/server-side/Data/UserJson.json";
                string existingJson = File.ReadAllText(filePath);
                JArray jsonArray = JArray.Parse(existingJson);
                JObject newJsonObject = new JObject
                {
                    { "Address", "5678 Oak Street, Metropolis" },
                    { "UserID", 102 },
                    { "firstName", "Jane" },
                    { "lastName", "Smith" },
                    { "UserEmail", "jane.smith@example.com" },
                    { "Passcode", "password456" },
                    { "SmartDevice", new JObject { { "SmartMeterID", 502 }, { "SmartMeterData", "27.3 kWh" } } }
                };

                jsonArray.Add(newJsonObject);
                string updatedJson = JsonConvert.SerializeObject(jsonArray, Formatting.Indented);
                File.WriteAllText(filePath, updatedJson);
                return true;
            }
            catch (Exception e){
                Console.WriteLine("Error Writing to File: " + e.ToString());
                return false;
            } 
        }

        public bool TestListToJson()
        {
            // Step 3: Create test data
            UserData testUsers = new UserData
            {
                UserID = 101,
                firstName = "John",
                lastName = "Doe",
                Address = "1234 Elm Street, Springfield",
                UserEmail = "john.doe@example.com",
                Passcode = "password123",
                SmartDevice = new SmartDevice
                {
                    SmartMeterID = 501,
                    SmartMeterData = "23.5 kWh"
                }
            };
            ListToJson(testUsers);
            return true;
        }
    }
}
