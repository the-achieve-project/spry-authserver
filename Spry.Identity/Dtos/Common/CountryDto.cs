namespace Spry.AuthServer.Dtos.Common
{
#nullable disable
    public class CountryDto
    {
        private string code;

        public string Flag { get; set; }
        public string Country { get; set; }
        public string Code { get => code; set => code = value; }
    }
}
