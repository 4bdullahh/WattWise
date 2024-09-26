
namespace SmartMeter
{
    public interface IUserMessageRepo
    {
        public UserData GetById(int UserID);
        public UserData AddUserData(UserData userData);
        public UserData UpdateData(UserData userData);


    }

}
