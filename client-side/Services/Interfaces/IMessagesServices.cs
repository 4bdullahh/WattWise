using client_side.Models;
using NetMQ;

namespace client_side.Services.Interfaces
{
    public interface IMessagesServices
    {
        public NetMQMessage SendReading
        (
            string clientAddress,
            UserModel userData,
            byte[] key,
            byte[] iv
        );
    }
}
