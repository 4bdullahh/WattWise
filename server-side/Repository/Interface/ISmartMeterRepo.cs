
using server_side.Services.Models;

namespace server_side.Repository.Interface;

public interface ISmartMeterRepo
{
    SmartDevice GetById(int SmartMeterID);
    SmartDevice UpdateMeterRepo(SmartDevice smartDevice);
}