using server_side.Repository.Interface;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace server_side.Repository
{
    public class UserMessageRepo : IUserMessageRepo
    {
        private List<UserData> usersList;
        public UserMessageRepo()
        {
            LoadUserData();
        }

        private void LoadUserData()
        {
            var jsonFilePath = "../Data/UserJson.json";

            if (File.Exists(jsonFilePath))
            {
                string jsonData = File.ReadAllText(jsonFilePath);
                JArray _users = JArray.Parse(jsonData);
                usersList = JsonConvert.DeserializeObject<List<UserData>>(_users.ToString());
            }
            else
            {
                File.Create(jsonFilePath);
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
            var result = ListToJson(users);
            return result;
        }

        // public bool UpdateData(UserData userData)
        // {

        //     var existingMessage = GetById(userData.UserID);
        //     if (existingMessage != null)
        //     {

        //         ListToJson(userData);
        //         return true;
        //     }
        //     else
        //     {
        //         return false;
        //     }
        // }


        public bool ListToJson(UserData users)
        {
            try
            {

                string filePath = "C:/Users/Muhammad.Mamoon/Documents/ENTERPRISE DESIGN/WattWise/server-side/Data/UserJson.json";
                string existingJson = File.ReadAllText(filePath);
                JArray userInfo = JArray.Parse(existingJson);

                JObject userDataObject = new JObject
                {
                    { "Address", users.Address},
                    { "UserID", users.UserID},
                    { "firstName", users.firstName},
                    { "lastName", users.lastName},
                    { "UserEmail", users.UserEmail},
                    { "Passcode", users.Passcode},
                    { "SmartDevice", new JObject {
                            { "SmartMeterID", users.SmartMeterID},
                            { "SmartMeterData", users.SmartMeterData }
                         }
                    }
                };

                userInfo.Add(userDataObject);
                string updatedJson = JsonConvert.SerializeObject(userInfo, Formatting.Indented);
                File.WriteAllText(filePath, updatedJson);
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
