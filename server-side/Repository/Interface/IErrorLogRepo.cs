using server_side.Repository.Models;

namespace server_side.Repository.Interface;

public interface IErrorLogRepo
{
    void LogError(ErrorLogMessage errorLogMessage);
}