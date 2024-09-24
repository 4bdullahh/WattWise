

namespace SmartMeter
{
    public class UserMessageRepo
    {
        private readonly List<UserData> userDatabase;
        public UserMessageRepo()
        {
            userDatabase = new List<UserData>();
        }

        //check file vailidty

        public UserData GetById(int UserID)
        {
            return userDatabase.FirstOrDefault(c => c.UserID == UserID);
        }


        //get electric amount

        public void AddUserData(UserData userData)
        {
            userDatabase.Add(userData);

        }

        public void UpdateData(UserData userData)
        {

            var existingMessage = GetById(userData.UserID);
            if (existingMessage != null)
            {
                userDatabase.Remove(existingMessage);
                userDatabase.Add(userData);
            }
        }

        public void DeleteData(UserData userData)
        {
            // Check If User is Admin
            // Reject if they are not

            var existingMessage = GetById(userData.UserID);
            if (existingMessage != null)
            {
                userDatabase.Remove(existingMessage);
                userDatabase.Add(userData);
            }
        }
        
        //WRITE SAVE TO FILE METHOD USE 
        
    }

}
