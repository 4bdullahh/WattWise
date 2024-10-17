
namespace server_side.Repository.Interface
{
    public interface IUserMessageRepo
    {
        UserData GetById(int UserID);
        bool AddUserData(UserData userData);
        
        bool UpdateUserData(UserData userData);

    }

}
