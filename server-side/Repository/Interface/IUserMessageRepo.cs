
namespace server_side.Repository.Interface
{
    public interface IUserMessageRepo
    {
        UserData GetById(int UserID);
        UserData AddUserData(UserData userData);
        
        UserData UpdateUserData(UserData userData);

    }

}
