
namespace server_side.Repository.Interface
{
    public interface IUserMessageRepo
    {
        public UserData GetById(int UserID);
        public bool AddUserData(UserData userData);
        public UserData UpdateData(UserData userData);
        public bool TestListToJson();


    }

}
