namespace server_side.Services.Interface
{
    public interface IUserServices
    {
         UserResponse UserOperations(string decryptedMessage);
    }
}

