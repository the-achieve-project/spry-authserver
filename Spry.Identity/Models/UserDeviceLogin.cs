namespace Spry.Identity.Models
{
#nullable disable
    public class UserDeviceLogin
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string Device { get; set; }
        public string Request { get; set; }
    }
}
