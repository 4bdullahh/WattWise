
namespace server_side.Repository.Interface
{
    public interface IUserMessageRepo
    {
        public UserData GetById(int UserID);
        public bool AddUserData(UserData userData);
        
        public bool UpdateUserData(UserData userData);

    }

}
