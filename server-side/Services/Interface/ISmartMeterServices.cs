using server_side.Services.Models;

namespace server_side.Services.Interface;

public interface ISmartMeterServices
{
    SmartMeterResponse UpdateMeterServices(string decryptedMessage);
}