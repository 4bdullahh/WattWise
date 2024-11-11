using System.Drawing;

namespace server_side.Repository.Models;

public class PowerGridTracker
{
    public double kwhLimit { get; set; }
    public List<int> clientList { get; set; } 
}