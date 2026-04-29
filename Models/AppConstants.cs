namespace AutoAssemblyMatcher.Models;

public static class AppConstants
{
    // File names for persisted JSON data
    public const string SettingsFileName = "settings.json";
    public const string RemappedFileName = "remapped.json";
    public const string IgnoredFileName = "ignored.json";

    // Type filtering prefix
    public const string ObfuscatedTypePrefix = "GClass";

    // UI defaults
    public const int DefaultWindowWidth = 1200;
    public const int DefaultWindowHeight = 800;
    public const int MaxDummyResults = 100;

    /// <summary>
    /// Returns the full path for a data file stored alongside the application executable.
    /// Moved from Settings.GetFilePath().
    /// </summary>
    public static string GetDataFilePath(string filename)
    {
        return Path.Combine(AppContext.BaseDirectory, filename);
    }
}
