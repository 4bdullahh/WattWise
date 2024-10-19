namespace server_side.Repository.Interface;

public interface ISaveData
{
    T ListToJson<T>(T data);
}