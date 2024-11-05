namespace client_side.Services.Interfaces;

public interface IClientServices
{
    public void StartClient();

    public Task ElectronServerAsync();
}