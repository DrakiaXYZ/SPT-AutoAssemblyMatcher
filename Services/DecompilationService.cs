namespace AutoAssemblyMatcher.Services;

using ICSharpCode.Decompiler;
using ICSharpCode.Decompiler.CSharp;
using ICSharpCode.Decompiler.CSharp.OutputVisitor;
using ICSharpCode.Decompiler.CSharp.Syntax;
using ICSharpCode.Decompiler.TypeSystem;

public class DecompilationService
{
    public CSharpDecompiler CreateDecompiler(string assemblyPath)
    {
        var settings = new DecompilerSettings()
        {
            OptionalArguments = false,
            NamedArguments = false,
            SortCustomAttributes = false,
            UseExpressionBodyForCalculatedGetterOnlyProperties = false,
        };

        return new CSharpDecompiler(assemblyPath, settings);
    }

    public string DecompileType(CSharpDecompiler decompiler, string fullClassName)
    {
        FullTypeName typeName = new FullTypeName(fullClassName);
        SyntaxTree syntaxTree = decompiler.DecompileType(typeName);

        foreach (var node in syntaxTree.Descendants.OfType<EntityDeclaration>())
        {
            node.Attributes.Clear();
        }

        int GetMemberPriority(EntityDeclaration member) => member switch
        {
            PropertyDeclaration => 1,
            ConstructorDeclaration => 2,
            MethodDeclaration => 3,
            FieldDeclaration => 4,
            TypeDeclaration => 5,
            _ => 6
        };

        var allTypes = syntaxTree.Descendants
            .OfType<TypeDeclaration>()
            .ToList();

        foreach (var type in allTypes)
        {
            var sortedMembers = type.Members.OrderBy(GetMemberPriority).ToList();

            type.Members.Clear();
            foreach (var member in sortedMembers)
            {
                type.Members.Add(member);
            }
        }

        var settings = FormattingOptionsFactory.CreateSharpDevelop();
        using (var writer = new StringWriter())
        {
            var visitor = new CSharpOutputVisitor(writer, settings);
            syntaxTree.AcceptVisitor(visitor);
            return writer.ToString();
        }
    }
}
