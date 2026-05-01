namespace AutoAssemblyMatcher.Services;

using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using AutoAssemblyMatcher.Models;

public class PersistenceService
{
    private readonly string _settingsPath;
    private readonly string _remappedPath;
    private readonly string _ignoredPath;

    public PersistenceService()
    {
        _settingsPath = AppConstants.GetDataFilePath(AppConstants.SettingsFileName);
        _remappedPath = AppConstants.GetDataFilePath(AppConstants.RemappedFileName);
        _ignoredPath = AppConstants.GetDataFilePath(AppConstants.IgnoredFileName);
    }

    // --- Settings ---

    public AppSettings? LoadSettings()
    {
        if (File.Exists(_settingsPath))
        {
            try
            {
                var options = new JsonSerializerOptions { AllowTrailingCommas = true };
                return JsonSerializer.Deserialize<AppSettings>(File.ReadAllText(_settingsPath), options);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to load settings: {ex.Message}");
            }
        }

        return null;
    }

    public void SaveSettings(AppSettings settings)
    {
        SaveToJson(_settingsPath, settings);
    }

    // --- Mappings ---

    public (HashSet<string> UsedNames, HashSet<string> MappedNames) LoadMappings(string mappingFilePath)
    {
        var usedNames = new HashSet<string>();
        var mappedNames = new HashSet<string>();

        var options = new JsonDocumentOptions { AllowTrailingCommas = true, CommentHandling = JsonCommentHandling.Skip };

        var mappings = JsonNode.Parse(File.ReadAllText(mappingFilePath), documentOptions: options).AsObject();
        foreach (var property in mappings)
        {
            var newName = GetFullName(property.Value["NewName"].ToString(), property.Value["NewNamespace"]?.ToString());
            usedNames.Add(newName);
            mappedNames.Add(property.Key);
        }

        return (usedNames, mappedNames);
    }

    // --- Ignored ---

    public HashSet<string> LoadIgnored()
    {
        if (File.Exists(_ignoredPath))
        {
            var json = File.ReadAllText(_ignoredPath);
            try
            {
                var options = new JsonSerializerOptions { AllowTrailingCommas = true };
                var list = JsonSerializer.Deserialize<List<string>>(json, options);
                return new HashSet<string>(list ?? new List<string>());
            }
            catch (Exception ex)
            {
                if (json.Length > 0)
                {
                    MessageBox.Show($"Exception loading ignored.json, verify or delete file. Exiting\n\n{ex.Message}");
                    Environment.Exit(0);
                }
            }
        }

        return new HashSet<string>();
    }

    public void SaveIgnored(HashSet<string> ignored)
    {
        SaveToJson(_ignoredPath, ignored.ToList());
    }

    // --- Remapped ---

    public Dictionary<string, RemapEntry> LoadRemapped(HashSet<string> usedNames)
    {
        var remapped = new Dictionary<string, RemapEntry>();

        if (File.Exists(_remappedPath))
        {
            var json = File.ReadAllText(_remappedPath);
            try
            {
                var options = new JsonSerializerOptions { AllowTrailingCommas = true };
                remapped = JsonSerializer.Deserialize<Dictionary<string, RemapEntry>>(json, options)
                    ?? new Dictionary<string, RemapEntry>();
            }
            catch (Exception ex)
            {
                if (json.Length > 0)
                {
                    MessageBox.Show($"Exception loading remapped.json, verify or delete file. Exiting\n\n{ex.Message}");
                    Environment.Exit(0);
                }
            }
        }

        foreach (var remap in remapped.Values)
        {
            var newName = GetFullName(remap.NewName, remap.NewNamespace);
            usedNames.Add(newName);
        }

        return remapped;
    }

    public void SaveRemapped(Dictionary<string, RemapEntry> remapped)
    {
        SaveToJson(_remappedPath, remapped);
    }

    // --- Cleaning ---

    public Dictionary<string, RemapEntry> CleanRemapped(
        Dictionary<string, RemapEntry> remapped,
        HashSet<string> mappedNames,
        IEnumerable<string> assemblyTypeNames)
    {
        // To make it so the user doesn't need to constantly clean up their remapped file, remove anything that exists
        // in either the Mappings file, or no longer exists in the assembly
        return remapped.Where(x => !mappedNames.Contains(x.Key) && assemblyTypeNames.Contains(x.Key)).ToDictionary();
    }

    // --- Helpers ---

    private void SaveToJson<T>(string path, T obj)
    {
        var options = new JsonSerializerOptions { WriteIndented = true, IndentSize = 4, Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping };
        string json = JsonSerializer.Serialize(obj, options);
        File.WriteAllText(path, json);
    }

    public static string GetFullName(string name, string? ns)
    {
        if (ns != null)
        {
            name = $"{ns}.{name}";
        }

        return name;
    }
}
