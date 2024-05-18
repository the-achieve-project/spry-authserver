namespace Spry.Identity.Utility
{
    public static class AppVariables
    {
        public const string Development = "Development";
        public const string Production = "Production";
        public static string CurrentEnvironment => Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")!;
    }
}
