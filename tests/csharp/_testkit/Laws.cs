using System.Collections.Frozen;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Rasm.TestKit;

// --- [TYPES] --------------------------------------------------------------------------------
// One attribute marks every law-to-subject correspondence; AllowMultiple lets one spec class or
// method cover several subjects, and the optional Member narrows coverage to a named symbol.
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
public sealed class LawAttribute(Type subject, string name) : Attribute {
    public Type Subject { get; } = subject;
    public string Name { get; } = name;
    public string? Member { get; init; }
}

// --- [MODELS] -------------------------------------------------------------------------------
// LawRecord is the flattened correspondence: subject type, law name, optional covered member, and
// the spec type that declared it. Member-less records cover the subject's whole public surface.
public readonly record struct LawRecord(Type Subject, string Name, Option<string> Member, Type DeclaringType) {
    // Coverage credits the simple symbol name: a member-scoped law covers `Subject.Member`, an
    // unscoped law covers the subject type name itself.
    public string CoveredSymbol => Member.IfNone(Subject.Name);
}

// SutTarget pairs the compiled assembly under test with the exemption set derived from its own
// `[CspExempt]`/`[CspScope(Tooling)]` production sites — never a parallel catalog.
public readonly record struct SutTarget(Assembly Assembly, FrozenSet<string> ExemptNames);

// --- [SERVICES] -----------------------------------------------------------------------------
public static class Laws {
    // Production-site exemption vocabulary lives in Csp.Contracts; reflection matches it by name so
    // the gate stays decoupled from a hard type reference at the foreign-vocabulary boundary.
    private const string ExemptAttribute = "Rasm.Csp.CspExemptAttribute";
    private const string ScopeAttribute = "Rasm.Csp.CspScopeAttribute";
    private const int ToolingScope = 6;

    // Record-synthesized and compiler-emitted members carry no law obligation; their simple names
    // and prefixes are stable across the generators that emit them.
    private static readonly FrozenSet<string> SynthesizedNames =
        new[] { "Equals", "GetHashCode", "ToString", "Deconstruct", "Clone", "PrintMembers", "EqualityContract", "op_Equality", "op_Inequality" }
            .ToFrozenSet(StringComparer.Ordinal);

    // ScanAssembly folds every `[Law]` on every type and method into one flat manifest; class-level
    // laws and method-level laws share the same record shape so coverage reads one stream.
    public static Seq<LawRecord> ScanAssembly(Assembly specAssembly) {
        ArgumentNullException.ThrowIfNull(argument: specAssembly);
        return toSeq(specAssembly.GetTypes())
            .Bind(type => Records(type.GetCustomAttributes<LawAttribute>(inherit: false), type)
                + toSeq(type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly))
                    .Bind(method => Records(method.GetCustomAttributes<LawAttribute>(inherit: false), type)));
    }

    // Sut admits the compiled SUT assembly and derives its exemption set in one pass over the
    // assembly's own `[CspExempt]`/`[CspScope(Tooling)]` declarations — no exemption is authored here.
    public static SutTarget Sut(Assembly sutAssembly) {
        ArgumentNullException.ThrowIfNull(argument: sutAssembly);
        return new SutTarget(
            Assembly: sutAssembly,
            ExemptNames: toSeq(sutAssembly.GetTypes())
                .Bind(ExemptNamesOf)
                .ToFrozenSet(StringComparer.Ordinal));
    }

    // AssertCoverage gates every SUT public symbol against the manifest plus the derived exemptions,
    // returning one `Fin<Unit>` whose `ManyErrors` body lists each uncovered symbol by name.
    public static Fin<Unit> AssertCoverage(SutTarget target, Seq<LawRecord> manifest) {
        FrozenSet<string> covered = manifest.Map(static record => record.CoveredSymbol).ToFrozenSet(StringComparer.Ordinal);
        Seq<string> uncovered = PublicSurface(target.Assembly)
            .Filter(symbol => !covered.Contains(symbol) && !target.ExemptNames.Contains(symbol))
            .Distinct()
            .Order(comparer: StringComparer.Ordinal)
            .AsIterable()
            .ToSeq();
        Seq<Error> gaps = uncovered.Map(static symbol => Error.New(
            $"law coverage gap in '{symbol}': no [Law] covers this public symbol and no production [CspExempt]/[CspScope(Tooling)] exempts it"));
        return uncovered.IsEmpty
            ? Fin.Succ(value: unit)
            : Fin.Fail<Unit>(error: Error.Many(errors: gaps));
    }

    // --- [OPERATIONS]
    // PublicSurface is exported types plus their declared-only public members, minus the
    // record-synthesized, compiler-generated, and `[Union]`/`[SmartEnum]` generated-case symbols.
    private static Seq<string> PublicSurface(Assembly assembly) =>
        toSeq(assembly.GetExportedTypes())
            .Filter(static type => !IsGenerated(type))
            .Bind(type => type.Name.Cons(Members(type)));

    private static Seq<string> Members(Type type) =>
        IsClosedFamily(type)
            ? Seq<string>()
            : toSeq(type.GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly))
                .Filter(member => member is not (ConstructorInfo or Type)
                    && !SynthesizedNames.Contains(member.Name)
                    && !IsGenerated(member)
                    && !(member is MethodInfo { IsSpecialName: true }))
                .Map(static member => member.Name);

    private static Seq<LawRecord> Records(IEnumerable<LawAttribute> attributes, Type declaringType) =>
        toSeq(attributes).Map(attribute => new LawRecord(
            Subject: attribute.Subject,
            Name: attribute.Name,
            Member: Optional(attribute.Member),
            DeclaringType: declaringType));

    // A type carrying any `[Union]`/`[SmartEnum]` Thinktecture marker owns a closed family; its
    // generated cases and dispatch members are derivation, not authored surface, so coverage credits
    // the family type alone and skips the generated members.
    private static bool IsClosedFamily(Type type) =>
        type.GetCustomAttributes(inherit: false).Any(static attribute =>
            attribute.GetType().FullName is string name
            && (name.StartsWith("Thinktecture.Union", StringComparison.Ordinal)
                || name.StartsWith("Thinktecture.AdHocUnion", StringComparison.Ordinal)
                || name.StartsWith("Thinktecture.SmartEnum", StringComparison.Ordinal)));

    private static bool IsGenerated(MemberInfo member) =>
        member.IsDefined(typeof(CompilerGeneratedAttribute), inherit: false)
        || member.Name.Contains('<', StringComparison.Ordinal)
        || member.Name.Contains('$', StringComparison.Ordinal);

    private static Seq<string> ExemptNamesOf(Type type) =>
        IsTypeExempt(type)
            ? type.Name.Cons(toSeq(type.GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly)).Map(static member => member.Name))
            : toSeq(type.GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly))
                .Filter(IsMemberExempt)
                .Map(static member => member.Name);

    private static bool IsTypeExempt(Type type) =>
        type.GetCustomAttributes(inherit: false).Any(IsExemptAttribute);

    private static bool IsMemberExempt(MemberInfo member) =>
        member.GetCustomAttributes(inherit: false).Any(IsExemptAttribute);

    // A `[CspExempt]` of any axis, or a `[CspScope(Tooling)]`, removes the symbol from the coverage
    // obligation; the scope ordinal mirrors the production `CspScope.Tooling` member.
    private static bool IsExemptAttribute(object attribute) =>
        attribute.GetType().FullName switch {
            ExemptAttribute => true,
            ScopeAttribute => attribute.GetType().GetProperty("Scope")?.GetValue(attribute) is int scope && scope == ToolingScope,
            _ => false,
        };
}
