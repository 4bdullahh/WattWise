public class UserResponse
{
    public int UserID { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Address;
    public string UserEmail { get; set; }
    public string? Topic { get; set; }

    public bool Successs { get; set; } = false;
    public string Hash { get; set; } 
    public string? Message { get; set; }

}