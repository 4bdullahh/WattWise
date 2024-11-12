using Microsoft.Extensions.Hosting;
using server_side.Repository.Interface;

public class CostUpdateService : IHostedService, IDisposable
{
    private Timer _timer;
    private readonly ICalculateCost _calculateCostService;
    private readonly ISmartMeterRepo _smartDevices; 

    public CostUpdateService(ICalculateCost calculateCostService, ISmartMeterRepo smartDevices)
    {
        _calculateCostService = calculateCostService;
        _smartDevices = smartDevices;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        Console.WriteLine("CostUpdateService starting...");
        if (cancellationToken.IsCancellationRequested)
        {
            return Task.CompletedTask;
        }
        _timer = new Timer(UpdateCosts, null, TimeSpan.Zero, TimeSpan.FromMinutes(1));
        return Task.CompletedTask;
    }

    public void UpdateCosts(object state)
    {
        Console.WriteLine("UpdateCosts running...");
    
        var smartDevices = _smartDevices.LoadSmartMeterData();
        if (smartDevices == null)
        {
            Console.WriteLine("No smart devices data available.");
            return;
        }

        foreach (var device in smartDevices)
        {
            try
            {
                _calculateCostService.getCurrentBill(device);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing device {device}: {ex.Message}");
            }
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