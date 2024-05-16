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

    public class GoogleAuthenticationOptions
    {
        public string Client_Id { get; set; }
        public string Client_Secret { get; set; }
        public string Project_Id { get; set; }
        public string Auth_Uri { get; set; }
        public string Token_Uri { get; set; }
        public string Auth_Provider_X509_Cert_Url { get; set; }
        public List<string> Redirect_Uris { get; set; }

    }

    public class MicrosoftAuthOptions
    {
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
    }

    public class YahooAuthOptions : MicrosoftAuthOptions
    {
        public string Authority { get; set; }
        public string CallbackPath { get; set; }
        public string ResponseType { get; set; }
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
