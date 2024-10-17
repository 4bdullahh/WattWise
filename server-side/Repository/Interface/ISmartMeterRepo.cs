
using server_side.Services.Models;

namespace server_side.Repository.Interface;

public interface ISmartMeterRepo
{
    SmartMeterResponse UpdateMeterRepo(SmartDevice smartDevice);
}