
using server_side.Services.Interface;

namespace server_side.Controller
{
    public class MessageController
    {
        private readonly IMessageServices _messageServices;

        /*
         * Class Documentation:
            This class is the controller who connects to program
         */
        public MessageController(IMessageServices messageServices)
        {
            _messageServices = messageServices;
        }

        public void ReceiveMessage()
        {
            _messageServices.ReceiveMessageServices();
        }
    }

}
