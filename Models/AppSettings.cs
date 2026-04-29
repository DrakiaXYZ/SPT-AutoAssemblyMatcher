namespace AutoAssemblyMatcher.Models;

public class AppSettings
{
    public AppSettings() { }

    public AppSettings(string assemblyDll, string dummyDll, string mappingFile)
    {
        AssemblyDll = assemblyDll;
        DummyDll = dummyDll;
        MappingFile = mappingFile;
    }

    public string AssemblyDll { get; set; } = string.Empty;
    public string DummyDll { get; set; } = string.Empty;
    public string MappingFile { get; set; } = string.Empty;
    public int Zoom { get; set; } = 0;
    public int WindowWidth { get; set; } = AppConstants.DefaultWindowWidth;
    public int WindowHeight { get; set; } = AppConstants.DefaultWindowHeight;
}
