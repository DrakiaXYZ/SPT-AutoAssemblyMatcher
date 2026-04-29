namespace AutoAssemblyMatcher.Models;

using System.Text.Json.Serialization;

public class RemapEntry
{
    public RemapEntry(string newName)
    {
        NewName = newName;
    }

    public string NewName { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? NewNamespace { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public bool HasChildClasses { get; set; } = false;

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Dictionary<string, Dictionary<string, string>>? NestedTypes { get; set; }
}
