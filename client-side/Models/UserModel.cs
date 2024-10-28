using client_side.Models;

namespace client_side.Models
{
    public class UserModel : SmartDevice
    {
        public int UserID { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Address;
        public string? UserEmail { get; set; }
        public string? Passcode { get; set; }
        public SmartDevice? SmartDevice { get; set; }
        public string? Hash { get; set; }
        public string? Topic { get; set; }
        public string? CustomerType { get; set; }
    }
    
}

