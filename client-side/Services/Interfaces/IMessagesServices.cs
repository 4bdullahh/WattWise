using client_side.Models;
using NetMQ;

namespace client_side.Services.Interfaces
{
    public interface IMessagesServices
    {
        public NetMQMessage SendReading <T>
        (
            string clientAddress,
            T modelData,
            byte[] key,
            byte[] iv
        );
    }
}
