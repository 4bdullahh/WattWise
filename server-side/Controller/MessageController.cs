
namespace SmartMeter
{
    public class MessageController
    {
        private readonly IUserServices _userServices;

        public MessageController(IUserServices userServices)
        {
            _userServices = userServices;
        }

        public void ReceiveMessage()
        {
            _userServices.ReceiveMessageServices();
        }

    }

}

