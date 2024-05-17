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
        public const string GoogleAuthentication = "GoogleAuthentication";
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
        public const string MicrosoftAuthentication = "MicrosoftAuthentication";
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
    }

    public class YahooAuthOptions : MicrosoftAuthOptions
    {
        public const string YahooAuthentication = "YahooAuthentication";
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
