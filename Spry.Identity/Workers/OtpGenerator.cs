using Spry.Identity.Utility;

namespace Spry.Identity.Workers
{
    public static class OtpGenerator
    {
        public static string Create()
        {
            var sg = new StringGenerator(useNumericCharacters: true, useSpecialCharacters: false,
                                            useLowerCaseCharacters: false, useUpperCaseCharacters: false);
            return sg.Generate(6);
        }
    }
}
