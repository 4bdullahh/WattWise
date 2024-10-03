public class UserResponse
{
    public int UserID { get; set; }
    public string firstName { get; set; }
    public string lastName { get; set; }
    public string Address;
    public string UserEmail { get; set; }
    public string? Topic { get; set; }

    public bool Successs { get; set; } = false;
    public string? Message { get; set; }

}