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
}
