namespace server_side.Repository.Interface;

public interface IHashHandle
{
    string HashUserData(string userData);
    bool ValidateHashData(string hashedData, string userData);
}