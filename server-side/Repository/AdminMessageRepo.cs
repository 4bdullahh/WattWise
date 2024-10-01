
namespace server_side.Repository
{
    public class AdminMessageRepo
    {
        private readonly List<UserData> userDatabase;

        public AdminMessageRepo()
        {
            userDatabase = new List<UserData>();
        }

        public UserData GetById(int UserID)
        {
            return userDatabase.FirstOrDefault(c => c.UserID == UserID);
        }

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

            var existingMessage = GetById(userData.UserID);
            if (existingMessage != null)
            {
                userDatabase.Remove(existingMessage);
                userDatabase.Add(userData);
            }
        }
    }

}
