
using server_side.Service.Interface;

namespace server_side
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


using server_side.Services.Interface;
namespace server_side.Controller
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

