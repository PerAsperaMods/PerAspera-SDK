namespace PerAspera.ModSDK.Systems
{
    /// <summary>
    /// SDK version information and compatibility
    /// </summary>
    public static class VersionInfo
    {
        public const string SDK_VERSION = "1.0.0";
        public const string API_VERSION = "1.0.0";
        public const string COMPATIBLE_GAME_VERSION = "1.4+";
        
        /// <summary>
        /// Get complete version information
        /// </summary>
        public static string GetFullVersion()
        {
            return $"PerAspera ModSDK v{SDK_VERSION} (API {API_VERSION}) - Game {COMPATIBLE_GAME_VERSION}";
        }

        /// <summary>
        /// Get short version string
        /// </summary>
        public static string GetShortVersion()
        {
            return $"v{SDK_VERSION}";
        }

        /// <summary>
        /// Check if a game version is compatible
        /// </summary>
        public static bool IsGameVersionCompatible(string gameVersion)
        {
            // Simple version check - can be enhanced with proper semantic versioning
            return !string.IsNullOrEmpty(gameVersion) && 
                   (gameVersion.StartsWith("1.4") || gameVersion.StartsWith("1.5") || gameVersion.StartsWith("1.6"));
        }
    }
}