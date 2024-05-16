namespace Spry.Identity.Utility
{
    public static class AppVariables
    {
        public static string CurrentEnvironment => Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")!;
    }
}
