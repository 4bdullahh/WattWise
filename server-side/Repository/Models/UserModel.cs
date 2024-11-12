public class UserData : SmartDevice
{
    public int UserID { get; set; }
    public string? firstName { get; set; }
    public string? lastName { get; set; }
    public string? Address;
    public string? UserEmail { get; set; }
    public SmartDevice? SmartDevice { get; set; }
    public string? Topic { get; set; }
    
    public string? Hash { get; set; }
    public string? CustomerType { get; set; }
}


