namespace Spry.Identity.SeedWork
{
#nullable disable
    public class IdentityServerSettings
    {
        public string CertificatePasswordProd { get; set; }
        public const string Settings = "IdentityServer";
        public string[] Audiences { get; set; }
        public string[] AchieveAudiences { get; set; }
        public string[] PayrollClients { get; set; }
    }

    public class EventBusSettings
    {
        public const string settings = "EventBus";
        public string Connection { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string RetryCount { get; set; }
    }
}
