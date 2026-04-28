// Required NuGet package: AsmResolver.DotNet

using AsmResolver.DotNet;
using AsmResolver.DotNet.Signatures.Types;

/// <summary>
/// Compares two compiled .NET assemblies (DLLs) and returns a percentage
/// match score (0.0 – 100.0).
///
/// Unlike the System.Reflection version, this class uses AsmResolver to read
/// raw PE metadata without loading or executing any assembly code.  This avoids
/// TypeLoadException, StackOverflowException, and all other runtime resolution
/// problems that occur when an assembly's dependencies are unavailable.
///
/// Scoring rules:
///   - Inheritance count mismatch                              => 0 % immediately.
///   - Public methods whose name does NOT start with "s?method_": matched by name + return type + parameter types.
///   - Methods whose name starts with "s?method_"               : matched by return type + parameter types only.
///   - Fields                                                   : matched by field type.
///   - Properties                                               : matched by property type.
///
/// Missing members in assembly2 and extra members in assembly2 both count
/// negatively.  The final score is clamped to [0, 100].
/// </summary>
public static class AssemblyComparator
{
    // -------------------------------------------------------------------------
    // Public entry points
    // -------------------------------------------------------------------------

    /// <summary>
    /// Reads both DLLs from disk and compares the first public class in each.
    /// </summary>
    public static double CalculateFromFiles(string dllPath1, string dllPath2)
    {
        var module1 = ModuleDefinition.FromFile(dllPath1);
        var module2 = ModuleDefinition.FromFile(dllPath2);
        return CalculateFromModules(module1, module2);
    }

    /// <summary>
    /// Reads both DLLs from in-memory byte arrays and compares the first
    /// public class in each.
    /// </summary>
    public static double CalculateFromBytes(byte[] dllBytes1, byte[] dllBytes2)
    {
        var module1 = ModuleDefinition.FromImage(
            AsmResolver.PE.PEImage.FromBytes(dllBytes1));
        var module2 = ModuleDefinition.FromImage(
            AsmResolver.PE.PEImage.FromBytes(dllBytes2));
        return CalculateFromModules(module1, module2);
    }

    /// <summary>
    /// Compares the first public concrete class found in each
    /// <see cref="ModuleDefinition"/>.
    /// </summary>
    public static double CalculateFromModules(ModuleDefinition module1, ModuleDefinition module2)
    {
        var class1 = GetFirstClass(module1);
        var class2 = GetFirstClass(module2);

        if (class1 == null || class2 == null)
            return 0.0;

        return Calculate(class1, class2);
    }

    // -------------------------------------------------------------------------
    // Core comparison
    // -------------------------------------------------------------------------

    public static double Calculate(TypeDefinition class1, TypeDefinition class2)
    {
        // ── 1. Inheritance count guard ────────────────────────────────────────
        if (GetBaseCount(class1) != GetBaseCount(class2))
            return 0.0;

        // ── 2. Collect members ────────────────────────────────────────────────
        var (publicMethods1, privateMethods1) = SplitMethods(class1);
        var (publicMethods2, privateMethods2) = SplitMethods(class2);

        var fields1 = GetFields(class1);
        var fields2 = GetFields(class2);

        var props1 = GetProperties(class1);
        var props2 = GetProperties(class2);

        // ── 3. Score each category ────────────────────────────────────────────
        int totalPoints = 0;
        int earnedPoints = 0;

        ScoreCategory(
            publicMethods1.Select(m => PublicMethodKey(m, class1)).ToList(),
            publicMethods2.Select(m => PublicMethodKey(m, class2)).ToList(),
            ref totalPoints, ref earnedPoints);

        ScoreCategory(
            privateMethods1.Select(m => AnonymousMethodKey(m, class1)).ToList(),
            privateMethods2.Select(m => AnonymousMethodKey(m, class2)).ToList(),
            ref totalPoints, ref earnedPoints);

        ScoreCategory(
            fields1.Select(f => FieldKey(f, class1)).ToList(),
            fields2.Select(f => FieldKey(f, class2)).ToList(),
            ref totalPoints, ref earnedPoints);

        ScoreCategory(
            props1.Select(p => PropertyKey(p, class1)).ToList(),
            props2.Select(p => PropertyKey(p, class2)).ToList(),
            ref totalPoints, ref earnedPoints);

        if (totalPoints == 0)
            return 100.0;

        double raw = (double)earnedPoints / totalPoints * 100.0;
        return Math.Max(0.0, Math.Min(100.0, raw));
    }

    /// <summary>
    /// Scores <paramref name="reference"/> against every class in
    /// <paramref name="module"/> and returns all candidates whose score
    /// is greater than 0, ordered from highest to lowest score.
    /// </summary>
    /// <param name="reference">The type to compare against.</param>
    /// <param name="module">The module whose types are candidates.</param>
    /// <returns>
    /// A list of <c>(TypeDefinition, score)</c> pairs for every type in
    /// <paramref name="module"/> that scores above 0 %, sorted descending.
    /// </returns>
    public static List<(TypeDefinition Type, double Score)> Calculate(
        TypeDefinition reference,
        ModuleDefinition module)
    {
        var results = new List<(TypeDefinition Type, double Score)>();

        foreach (var candidate in module.TopLevelTypes)
        {
            // Skip non-class types (interfaces, enums, structs, delegates).
            if (!candidate.IsClass)
                continue;

            double score = Calculate(reference, candidate);

            if (score > 0.0)
                results.Add((candidate, score));
        }

        results.Sort((a, b) => b.Score.CompareTo(a.Score));
        return results;
    }

    // -------------------------------------------------------------------------
    // Scoring (identical algorithm to previous versions)
    // -------------------------------------------------------------------------

    private static void ScoreCategory(
        List<string> keys1,
        List<string> keys2,
        ref int totalPoints,
        ref int earnedPoints)
    {
        var remaining2 = new List<string>(keys2);
        int matched = 0;

        foreach (var key in keys1)
        {
            int idx = remaining2.IndexOf(key);
            if (idx >= 0)
            {
                matched++;
                remaining2.RemoveAt(idx);
            }
        }

        totalPoints += Math.Max(keys1.Count, keys2.Count);
        earnedPoints += matched;
    }

    // -------------------------------------------------------------------------
    // AsmResolver helpers
    // -------------------------------------------------------------------------

    /// <summary>
    /// Returns the first public, non-abstract, non-nested, non-compiler-generated
    /// class defined in the module's top-level types.
    /// </summary>
    private static TypeDefinition? GetFirstClass(ModuleDefinition module)
        => module.TopLevelTypes.FirstOrDefault(t =>
               t.IsClass
            && t.IsPublic
            && !t.IsAbstract
            && !IsCompilerGenerated(t));

    /// <summary>
    /// Counts explicit base types: the base class (if not System.Object) plus
    /// any interfaces declared directly on the type.
    /// </summary>
    private static int GetBaseCount(TypeDefinition t)
    {
        int count = 0;

        if (t.BaseType != null
            && t.BaseType.FullName != "System.Object")
        {
            count++;
        }

        count += t.Interfaces.Count;

        return count;
    }

    /// <summary>
    /// Splits a type's methods into two buckets:
    ///   - Public bucket  : methods that are public and do NOT start with "s?method_".
    ///   - Private bucket : methods that start with "s?method_", OR are private/protected,
    ///                      regardless of whether the name starts with "s?method_".
    /// Excludes constructors, property/event accessors, and compiler-generated methods.
    /// </summary>
    private static (List<MethodDefinition> publicMethods,
                    List<MethodDefinition> privateMethods)
        SplitMethods(TypeDefinition t)
    {
        var publicList = new List<MethodDefinition>();
        var privateList = new List<MethodDefinition>();

        foreach (var m in t.Methods)
        {
            // Skip constructors and property/event accessors (IsSpecialName)
            if (m.IsConstructor
             || m.IsSpecialName)
            {
                continue;
            }

            bool hasMethodPrefix = m.Name is not null
                                && (
                                    m.Name.Value.StartsWith("method_", StringComparison.Ordinal)
                                    || m.Name.Value.StartsWith("smethod_", StringComparison.Ordinal));

            bool isExplicitImplementation = m.Name.Contains(".");
            bool isPrivate = m.IsPrivate || (!m.IsVirtual && (m.IsFamily || m.IsFamilyOrAssembly));

            if (!isExplicitImplementation && (hasMethodPrefix || isPrivate))
                privateList.Add(m);
            else
                publicList.Add(m);
        }

        return (publicList, privateList);
    }

    /// <summary>
    /// Returns fields declared on the type, excluding compiler-generated
    /// backing fields (e.g. auto-property storage).
    /// </summary>
    private static List<FieldDefinition> GetFields(TypeDefinition t)
        => t.Fields
            .Where(f => !IsCompilerGenerated(f))
            .ToList();

    private static List<PropertyDefinition> GetProperties(TypeDefinition t)
        => t.Properties.ToList();

    // -------------------------------------------------------------------------
    // Key builders
    // -------------------------------------------------------------------------

    private static string PublicMethodKey(MethodDefinition m, TypeDefinition owningType)
    {
        var genericParams = GenericParamNames(m);
        var paramTypes = m.Signature?.ParameterTypes.Select(t => TypeKey(t, genericParams, owningType))
                        ?? Enumerable.Empty<string>();

        var methodName = m.Name.ToString();
        var dotIndex = methodName.IndexOf('.');
        if (dotIndex >= 0)
        {
            methodName = methodName[dotIndex..];
        }

        return $"{methodName}|{TypeKey(m.Signature?.ReturnType, genericParams, owningType)}|({string.Join(",", paramTypes)})";
    }

    private static string AnonymousMethodKey(MethodDefinition m, TypeDefinition owningType)
    {
        var genericParams = GenericParamNames(m);
        var paramTypes = m.Signature?.ParameterTypes.Select(t => TypeKey(t, genericParams, owningType))
                        ?? Enumerable.Empty<string>();

        return $"{TypeKey(m.Signature?.ReturnType, genericParams, owningType)}|({string.Join(",", paramTypes)})";
    }

    private static string FieldKey(FieldDefinition f, TypeDefinition owningType)
    {
        // Fields cannot declare their own generic parameters; use the owning
        // type's parameters so that e.g. a field of type T resolves correctly.
        var genericParams = GenericParamNames(owningType);
        return TypeKey(f.Signature?.FieldType, genericParams, owningType);
    }

    private static string PropertyKey(PropertyDefinition p, TypeDefinition owningType)
    {
        var genericParams = GenericParamNames(owningType);
        return TypeKey(p.Signature?.ReturnType, genericParams, owningType);
    }

    /// <summary>
    /// Builds a name→positional-index map for a method's own generic parameters
    /// (e.g. void Foo&lt;T, U&gt;() → { "T"→"!0", "U"→"!1" }).
    /// </summary>
    private static Dictionary<string, string> GenericParamNames(MethodDefinition m)
    {
        var map = new Dictionary<string, string>(StringComparer.Ordinal);
        if (m.GenericParameters is null) return map;
        for (int i = 0; i < m.GenericParameters.Count; i++)
        {
            var name = m.GenericParameters[i].Name?.Value;
            if (name is not null)
                map[name] = $"!M{i}"; // prefix M = method-level, avoids clash with type params
        }
        return map;
    }

    /// <summary>
    /// Builds a name→positional-index map for a type's generic parameters
    /// (e.g. class Foo&lt;TKey, TValue&gt; → { "TKey"→"!T0", "TValue"→"!T1" }).
    /// </summary>
    private static Dictionary<string, string> GenericParamNames(TypeDefinition t)
    {
        var map = new Dictionary<string, string>(StringComparer.Ordinal);
        if (t.GenericParameters is null) return map;
        for (int i = 0; i < t.GenericParameters.Count; i++)
        {
            var name = t.GenericParameters[i].Name?.Value;
            if (name is not null)
                map[name] = $"!T{i}"; // prefix T = type-level
        }
        return map;
    }

    // -------------------------------------------------------------------------
    // Type key
    // -------------------------------------------------------------------------

    /// <summary>
    /// Produces a stable, assembly-agnostic string for an AsmResolver
    /// <see cref="TypeSignature"/>.  Because AsmResolver works purely from
    /// metadata, no assembly loading occurs here — the type name is read
    /// directly from the PE's string heap.
    ///
    /// Generic type parameters (e.g. T, TValue) are replaced with a positional
    /// placeholder (!T0, !T1, … for type params; !M0, !M1, … for method params)
    /// so that names that differ only in the parameter name still compare equal.
    ///
    /// Self-referencing types (where the type signature refers back to
    /// <paramref name="owningType"/>) are replaced with the placeholder "!Self"
    /// so that class names that differ between the two assemblies being compared
    /// do not prevent a match.
    /// </summary>
    private static string TypeKey(
        TypeSignature? sig,
        Dictionary<string, string>? genericParams = null,
        TypeDefinition? owningType = null)
    {
        if (sig is null)
            return "void";

        return sig switch
        {
            // Corlib primitives: use the CLR full name (e.g. "System.Int32")
            // so that "int" and "Int32" compare equal across modules.
            CorLibTypeSignature corLib
                => corLib.Type.ToString() ?? sig.FullName,

            // Generic type parameter (e.g. T, TValue, TKey).
            // Replace with a positional placeholder so that names that differ
            // only in the parameter name still compare equal.
            GenericParameterSignature gp
                => genericParams is not null && genericParams.TryGetValue(gp.Name ?? "", out var placeholder)
                    ? placeholder
                    : $"!{gp.ParameterType}{gp.Index}", // fallback: positional index from metadata

            // Constructed generics: name + all type arguments, e.g. "List`1[System.String]"
            // Check for self-reference before expanding arguments.
            GenericInstanceTypeSignature generic
                => IsSelfReference(generic.GenericType, owningType)
                    ? $"!Self`{generic.TypeArguments.Count}"
                    : $"{generic.GenericType.FullName}"
                     + $"[{string.Join(",", generic.TypeArguments.Select(t => TypeKey(t, genericParams, owningType)))}]",

            // Non-generic type reference — the most common place a self-reference appears
            // (e.g. a method returning MyClass, or a field of type MyClass).
            TypeDefOrRefSignature typeRef
                => IsSelfReference(typeRef.Type, owningType)
                    ? "!Self"
                    : typeRef.FullName,

            // Single-dimension arrays (most common)
            SzArrayTypeSignature szArr
                => $"{TypeKey(szArr.BaseType, genericParams, owningType)}[]",

            // Multi-dimension arrays
            ArrayTypeSignature arr
                => $"{TypeKey(arr.BaseType, genericParams, owningType)}[{new string(',', arr.Dimensions.Count - 1)}]",

            // By-ref (ref / out parameters)
            ByReferenceTypeSignature byRef
                => $"{TypeKey(byRef.BaseType, genericParams, owningType)}@",

            // Pointers
            PointerTypeSignature ptr
                => $"{TypeKey(ptr.BaseType, genericParams, owningType)}*",

            // Everything else
            _ => sig.FullName
        };
    }

    /// <summary>
    /// Returns true when <paramref name="typeRef"/> refers to the same type as
    /// <paramref name="owningType"/>, compared by namespace + name only so that
    /// types from two different modules (with different assembly tokens) still match.
    /// </summary>
    private static bool IsSelfReference(ITypeDefOrRef? typeRef, TypeDefinition? owningType)
    {
        if (typeRef is null || owningType is null)
            return false;

        return typeRef.Name == owningType.Name
            && typeRef.Namespace == owningType.Namespace;
    }

    // -------------------------------------------------------------------------
    // Compiler-generated detection
    // -------------------------------------------------------------------------

    /// <summary>
    /// Detects [CompilerGenerated] by reading the raw custom attribute constructor
    /// reference name — no type resolution or assembly loading required.
    /// </summary>
    private static bool IsCompilerGenerated(IHasCustomAttribute member)
        => member.CustomAttributes.Any(a =>
               a.Constructor?.DeclaringType?.FullName
               == "System.Runtime.CompilerServices.CompilerGeneratedAttribute");
}