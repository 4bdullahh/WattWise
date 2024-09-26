

namespace SmartMeter
{
    public class UserMessageRepo : IUserMessageRepo
    {
        private readonly List<UserData> userDatabase;
        public UserMessageRepo()
        {
            userDatabase = new List<UserData>();
        }

        //check file vailidty
        public UserData GetById(int UserID)
        {
            var user = userDatabase.FirstOrDefault(c => c.UserID == UserID);
            return user;
        }


        public UserData AddUserData(UserData userData)
        {
            userDatabase.Add(userData);
            return userData;
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

    }

}
