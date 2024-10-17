
using server_side.Services.Interface;

namespace server_side.Controller
{
    public class MessageController
    {
        private readonly IMessageServices _messageServices;

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
