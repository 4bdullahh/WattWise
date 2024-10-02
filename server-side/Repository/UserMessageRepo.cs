using server_side.Repository.Interface;
using Newtonsoft.Json;


namespace server_side.Repository
{
    public class UserMessageRepo : IUserMessageRepo
    {
        private List<UserData> _users;
        public UserMessageRepo()
        {
            LoadUserData();
        }

        private void LoadUserData()
        {
            // Path to your JSON file
            var jsonFilePath = "C:/Users/Muhammad.Mamoon/Documents/ENTERPRISE DESIGN/WattWise/server-side/Data/UserJson.json";

            if (File.Exists(jsonFilePath))
            {
                var jsonData = File.ReadAllText(jsonFilePath);
                _users = JsonConvert.DeserializeObject<List<UserData>>(jsonData);
            }
            else
            {
                _users = new List<UserData>();
            }
        }

        public UserData GetById(int UserID)
        {
            return _users.FirstOrDefault(user => user.UserID == UserID);
        }


        public UserData AddUserData(UserData userData)
        {
            ListToJson(userData);
            return userData;
        }

        public UserData UpdateData(UserData userData)
        {

            var existingMessage = GetById(userData.UserID);
            if (existingMessage != null)
            {

                _users.Remove(existingMessage);
                _users.Add(userData);
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
            string json = JsonConvert.SerializeObject(users);
            string filePath = "C:/Users/Muhammad.Mamoon/Documents/ENTERPRISE DESIGN/WattWise/server-side/Data/UserJson.json";
            File.WriteAllText(filePath, json);

            return true;
        }

        public bool TestListToJson()
        {
            // Step 3: Create test data
            List<UserData> testUsers = new List<UserData>
            {
                new UserData
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
                },
                new UserData
                {
                    UserID = 102,
                    firstName = "Jane",
                    lastName = "Smith",
                    Address = "5678 Oak Avenue, Shelbyville",
                    UserEmail = "jane.smith@example.com",
                    Passcode = "password456",
                    SmartDevice = new SmartDevice
                    {
                        SmartMeterID = 502,
                        SmartMeterData = "45.2 kWh"
                    }
                }
            };

            return true;
        }
    }
}
