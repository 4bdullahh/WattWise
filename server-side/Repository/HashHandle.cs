using server_side.Cryptography;
using server_side.Repository.Interface;

namespace server_side.Repository;

public class HashHandle : IHashHandle
{

    public string HashUserData(string userData)
    {
        return Cryptography.Cryptography.GenerateHash(userData);
    }
    
    public bool ValidateHashData(string hashedData, string originalData)
    {
        string hashedOriginalData = Cryptography.Cryptography.GenerateHash(originalData);
        return hashedData == hashedOriginalData;
    }
}