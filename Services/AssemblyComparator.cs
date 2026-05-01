namespace AutoAssemblyMatcher.Services;

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
    // Core comparison
    // -------------------------------------------------------------------------

    public static double Calculate(TypeDefinition class1, TypeDefinition class2)
    {
        // ── 1. Inheritance count guard ────────────────────────────────────────────
        if (GetBaseCount(class1) != GetBaseCount(class2))
            return 0.0;

        // ── 2. Collect members ────────────────────────────────────────────────
        var (publicMethods1, privateMethods1) = SplitMethods(class1);
        var (publicMethods2, privateMethods2) = SplitMethods(class2);

        var constructors1 = GetConstructors(class1);
        var constructors2 = GetConstructors(class2);

        var fields1 = GetFields(class1);
        var fields2 = GetFields(class2);

        var props1 = GetProperties(class1);
        var props2 = GetProperties(class2);

        // ── 3. Score each category ────────────────────────────────────────────
        int totalPoints = 0;
        int earnedPoints = 0;

        ScoreMethodCategory(
            publicMethods1.Select(m => MethodSignature.FromPublic(m, class1)).ToList(),
            publicMethods2.Select(m => MethodSignature.FromPublic(m, class2)).ToList(),
            ref totalPoints, ref earnedPoints);

        ScoreMethodCategory(
            privateMethods1.Select(m => MethodSignature.FromAnonymous(m, class1)).ToList(),
            privateMethods2.Select(m => MethodSignature.FromAnonymous(m, class2)).ToList(),
            ref totalPoints, ref earnedPoints);

        ScoreConstructorCategory(
            constructors1.Select(m => ConstructorSignature.From(m, class1)).ToList(),
            constructors2.Select(m => ConstructorSignature.From(m, class2)).ToList(),
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
    // Scoring
    // -------------------------------------------------------------------------

    /// <summary>
    /// Scores non-method members (fields, properties) as all-or-nothing per member.
    /// </summary>
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

    /// <summary>
    /// Scores methods with per-component granularity.
    ///
    /// For each method in <paramref name="sigs1"/>, finds the best candidate in
    /// <paramref name="sigs2"/> and scores it as follows:
    ///   +1 if the method name matches
    ///   +1 if the return type matches
    ///   +1 per matching parameter type (only if param counts are equal; otherwise 0)
    ///
    /// Total possible points per method = 2 + param count.
    /// Methods missing or extra in sigs2 earn 0 for all their possible points.
    /// </summary>
    private static void ScoreMethodCategory(
        List<MethodSignature> sigs1,
        List<MethodSignature> sigs2,
        ref int totalPoints,
        ref int earnedPoints)
    {
        var remaining2 = new List<MethodSignature>(sigs2);

        foreach (var m1 in sigs1)
        {
            int possiblePoints = 2 + m1.ParamTypes.Count;
            int bestMatch = -1;
            int bestScore = -1;

            for (int i = 0; i < remaining2.Count; i++)
            {
                var m2 = remaining2[i];

                int score = 0;
                if (m1.MethodName == m2.MethodName) score++;

                // If one side returns void and the other doesn't, this candidate
                // is disqualified entirely — don't pair them at all.
                bool m1Void = m1.ReturnType == "void";
                bool m2Void = m2.ReturnType == "void";
                if (m1Void != m2Void) continue;

                if (m1.ReturnType == m2.ReturnType) score++;

                int paramScore = ScoreParams(m1.ParamTypes, m2.ParamTypes);
                if (paramScore >= 0) score += paramScore;

                if (score > bestScore)
                {
                    bestScore = score;
                    bestMatch = i;

                    // Early exit if we already found a perfect match.
                    if (bestScore == possiblePoints) break;
                }
            }

            totalPoints += possiblePoints;

            if (bestMatch >= 0)
            {
                earnedPoints += bestScore;
                remaining2.RemoveAt(bestMatch);
            }
        }

        // Extra methods in sigs2 with no counterpart in sigs1 penalise the score.
        foreach (var extra in remaining2)
        {
            totalPoints += 2 + extra.ParamTypes.Count;
        }
    }

    /// <summary>
    /// Scores parameter lists pairwise by position, returning the number of
    /// matching positions.  Returns -1 if the parameter counts differ, which
    /// the caller treats as a disqualifying mismatch (scores 0 for the method).
    /// </summary>
    private static int ScoreParams(IReadOnlyList<string> params1, IReadOnlyList<string> params2)
    {
        if (params1.Count != params2.Count)
            return -1;

        int score = 0;
        for (int i = 0; i < params1.Count; i++)
        {
            if (params1[i] == params2[i])
                score++;
        }
        return score;
    }

    /// <summary>
    /// Scores constructors using the same per-parameter rules as methods.
    /// Constructors have no name or return type to score — only parameter types.
    /// Total possible points per constructor = param count.
    /// </summary>
    private static void ScoreConstructorCategory(
        List<ConstructorSignature> sigs1,
        List<ConstructorSignature> sigs2,
        ref int totalPoints,
        ref int earnedPoints)
    {
        var remaining2 = new List<ConstructorSignature>(sigs2);

        foreach (var c1 in sigs1)
        {
            int possiblePoints = c1.ParamTypes.Count;
            int bestMatch = -1;
            int bestScore = -1;

            for (int i = 0; i < remaining2.Count; i++)
            {
                // Parameter count must match exactly (same rule as methods).
                int paramScore = ScoreParams(c1.ParamTypes, remaining2[i].ParamTypes);
                if (paramScore >= 0 && paramScore > bestScore)
                {
                    bestScore = paramScore;
                    bestMatch = i;

                    if (bestScore == possiblePoints) break;
                }
            }

            totalPoints += possiblePoints;

            if (bestMatch >= 0)
            {
                earnedPoints += bestScore;
                remaining2.RemoveAt(bestMatch);
            }
        }

        // Extra constructors in sigs2 penalise the score.
        foreach (var extra in remaining2)
            totalPoints += extra.ParamTypes.Count;
    }

    // -------------------------------------------------------------------------
    // ConstructorSignature
    // -------------------------------------------------------------------------

    /// <summary>
    /// Holds the parameter type keys for a single constructor.
    /// Constructors have no name or return type to score.
    /// </summary>
    private sealed class ConstructorSignature
    {
        public IReadOnlyList<string> ParamTypes { get; }

        private ConstructorSignature(IReadOnlyList<string> paramTypes)
            => ParamTypes = paramTypes;

        public static ConstructorSignature From(MethodDefinition m, TypeDefinition owningType)
        {
            var genericParams = GenericParamNames(owningType);
            var paramTypes = m.Signature?.ParameterTypes
                              .Select(t => TypeKey(t, genericParams, owningType))
                              .ToList()
                             ?? [];

            return new ConstructorSignature(paramTypes);
        }
    }

    // -------------------------------------------------------------------------
    // MethodSignature — separates identity from parameter list
    // -------------------------------------------------------------------------

    /// <summary>
    /// Decomposes a method into its individually scored components:
    /// method name, return type, and a list of parameter type keys.
    /// </summary>
    private sealed class MethodSignature
    {
        /// <summary>
        /// The method name used for matching.
        /// For public methods: the declared name.
        /// For anonymous methods: empty string (name is irrelevant).
        /// </summary>
        public string MethodName { get; }

        /// <summary>The return type key.</summary>
        public string ReturnType { get; }

        /// <summary>Individual parameter type keys, one entry per parameter.</summary>
        public IReadOnlyList<string> ParamTypes { get; }

        private MethodSignature(string methodName, string returnType, IReadOnlyList<string> paramTypes)
        {
            MethodName = methodName;
            ReturnType = returnType;
            ParamTypes = paramTypes;
        }

        public static MethodSignature FromPublic(MethodDefinition m, TypeDefinition owningType)
        {
            var genericParams = GenericParamNames(m);
            var returnType = TypeKey(m.Signature?.ReturnType, genericParams, owningType);

            var methodName = m.Name?.ToString() ?? "";
            var dotIndex = methodName.IndexOf('.');
            if (dotIndex >= 0)
                methodName = methodName[dotIndex..];

            var paramTypes = m.Signature?.ParameterTypes
                              .Select(t => TypeKey(t, genericParams, owningType))
                              .ToList()
                             ?? [];

            return new MethodSignature(methodName, returnType, paramTypes);
        }

        public static MethodSignature FromAnonymous(MethodDefinition m, TypeDefinition owningType)
        {
            var genericParams = GenericParamNames(m);
            var returnType = TypeKey(m.Signature?.ReturnType, genericParams, owningType);

            var paramTypes = m.Signature?.ParameterTypes
                              .Select(t => TypeKey(t, genericParams, owningType))
                              .ToList()
                             ?? [];

            // Anonymous methods are matched by return type + params only;
            // the name is not scored so it is left empty.
            return new MethodSignature(string.Empty, returnType, paramTypes);
        }
    }

    // -------------------------------------------------------------------------
    // AsmResolver helpers
    // -------------------------------------------------------------------------

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
    /// Returns instance and static constructors declared on the type,
    /// excluding the default parameterless constructor if it is compiler-generated.
    /// </summary>
    private static List<MethodDefinition> GetConstructors(TypeDefinition t)
        => t.Methods
            .Where(m => m.IsConstructor && !IsCompilerGenerated(m))
            .ToList();

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
    ///
    /// Only the simple type Name (not FullName) is used for non-primitive types
    /// so that "Foo.Bar.EInteractState" and "Baz.EInteractState" compare equal.
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
            // Use Name (not FullName) so that types from different namespaces compare equal.
            GenericInstanceTypeSignature generic
                => IsSelfReference(generic.GenericType, owningType)
                    ? $"!Self`{generic.TypeArguments.Count}"
                    : $"{generic.GenericType.Name}"
                     + $"[{string.Join(",", generic.TypeArguments.Select(t => TypeKey(t, genericParams, owningType)))}]",

            // Non-generic type reference — the most common place a self-reference appears
            // (e.g. a method returning MyClass, or a field of type MyClass).
            // Use Name (not FullName) so that "Foo.Bar.EInteractState" compares
            // equal to "Baz.EInteractState" — only the simple type name is checked.
            TypeDefOrRefSignature typeRef
                => IsSelfReference(typeRef.Type, owningType)
                    ? "!Self"
                    : typeRef.Name,

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