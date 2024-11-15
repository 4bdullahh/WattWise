using server_side.Repository.Models;

namespace server_side.Repository.Interface;

public interface IPowerGridCalc
{
    SmartDevice CalculateGridOutage(SmartDevice smartDevice);
}