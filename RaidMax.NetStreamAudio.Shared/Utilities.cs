using System;

namespace RaidMax.NetStreamAudio.Shared
{
    /// <summary>
    /// Various and sundry helpers and utilities
    /// </summary>
    public static class Utilities
    {
        /// <summary>
        /// Indicates if the current environment is development
        /// </summary>
        public static bool IsDevelopment => Environment.GetEnvironmentVariable("DOTNETCORE_ENVIRONMENT") == "Development";
    }
}
