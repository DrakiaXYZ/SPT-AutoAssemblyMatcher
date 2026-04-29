namespace AutoAssemblyMatcher.Models;

using AsmResolver.DotNet;

public class AssemblyType
{
    public string Name { get; set; }
    public TypeDefinition Definition { get; set; }

    public AssemblyType(string name, TypeDefinition definition)
    {
        Name = name;
        Definition = definition;
    }
}
