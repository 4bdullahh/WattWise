namespace server_side.Services.Interface;

public interface IFolderPathServices
{
    string GetServerSideFolderPath();
    string GetWattWiseFolderPath();
    string GetClientFolderPath();

}