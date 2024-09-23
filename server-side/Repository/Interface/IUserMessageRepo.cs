
namespace SmartMeter
{
    public interface IUserMessageRepo
    {
        public UserData GetById(int UserID);
        public void AddUserData(UserData userData);
        public void UpdateData(UserData userData);


    }

}
