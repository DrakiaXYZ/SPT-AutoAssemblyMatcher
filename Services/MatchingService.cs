using AsmResolver.DotNet;
using NaturalSort.Extension;
using AutoAssemblyMatcher.Models;

namespace AutoAssemblyMatcher.Services;

/// <summary>
/// Encapsulates domain logic for type filtering, candidate matching,
/// and remap entry creation — extracted from MainForm.
/// </summary>
public class MatchingService
{
    /// <summary>
    /// Filters assembly types from the module based on multiple criteria:
    /// must have members, must not already be remapped/mapped/ignored,
    /// and must start with the obfuscated type prefix.
    /// </summary>
    public List<AssemblyType> LoadFilteredAssemblyTypes(
        ModuleDefinition module,
        Dictionary<string, RemapEntry> remapped,
        HashSet<string> mappedNames,
        HashSet<string> ignored)
    {
        return module
            .TopLevelTypes.Where(t =>
                (t.Methods.Count > 0 || t.Fields.Count > 0 || t.Properties.Count > 0) &&
                !remapped.Keys.Contains(t.Name) &&
                !mappedNames.Contains(t.Name) &&
                !ignored.Contains(t.Name) &&
                t.Name.ToString().StartsWith(AppConstants.ObfuscatedTypePrefix)
            )
            .Select(t => new AssemblyType(t.Name, t))
            .OrderBy(t => t.Name, StringComparer.OrdinalIgnoreCase.WithNaturalSort())
            .ToList();
    }

    /// <summary>
    /// Compares the reference type against all types in the dummy module,
    /// filters out already-used names, and returns scored candidates.
    /// </summary>
    public List<(AssemblyType Type, double Score)> FindCandidateMatches(
        TypeDefinition reference,
        ModuleDefinition dummyModule,
        HashSet<string> usedNames)
    {
        return AssemblyComparator.Calculate(reference, dummyModule)
            .Where(x => !usedNames.Contains(x.Type.FullName))
            .Select(x => (new AssemblyType(x.Type.Name, x.Type), x.Score))
            .ToList();
    }

    /// <summary>
    /// Creates a RemapEntry from two type definitions, handling backtick
    /// stripping for generic types, namespace assignment, and nested type detection.
    /// </summary>
    public RemapEntry CreateRemapEntry(TypeDefinition oldType, TypeDefinition newType)
    {
        // Assembly tool needs the new name to not have `1
        var newName = newType.Name.ToString();
        var newNameBacktickIndex = newName.IndexOf('`');
        if (newNameBacktickIndex >= 0)
        {
            newName = newName.Remove(newNameBacktickIndex);
        }

        var remap = new RemapEntry(newName);
        if (newType.Namespace is not null)
        {
            remap.NewNamespace = newType.Namespace;
        }
        if (oldType.NestedTypes.Any(x => !x.IsCompilerGenerated()))
        {
            remap.HasChildClasses = true;
            remap.NestedTypes = new();
            foreach (var nestedType in oldType.NestedTypes.Where(x => !x.IsCompilerGenerated()))
            {
                remap.NestedTypes[nestedType.Name] = new() { ["NewName"] = "" };
            }
        }

        return remap;
    }
}
