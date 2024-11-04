using Microsoft.Extensions.Hosting;

using server_side.Repository.Interface;

public class CostUpdateService : IHostedService, IDisposable
{
    private Timer _timer;
    private readonly ICalculateCost _calculateCostService;
    private readonly List<SmartDevice> _smartDevices; 

    public CostUpdateService(ICalculateCost calculateCostService, List<SmartDevice> smartDevices)
    {
        _calculateCostService = calculateCostService;
        _smartDevices = smartDevices;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _timer = new Timer(UpdateCosts, null, TimeSpan.Zero, TimeSpan.FromMinutes(1));
        return Task.CompletedTask;
    }

    private void UpdateCosts(object state)
    {
        foreach (var device in _smartDevices)
        {
            _calculateCostService.getCurrentBill(device);
            // We can write notifications for the update if we want
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _timer?.Change(Timeout.Infinite, 0);
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _timer?.Dispose();
    }
}