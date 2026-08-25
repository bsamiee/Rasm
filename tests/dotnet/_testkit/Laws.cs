using System.Collections.Frozen;
using System.Reflection;
using System.Runtime.CompilerServices;
using Rasm.Csp;

namespace Rasm.TestKit;

// --- [TYPES] ---------------------------------------------------------------------------
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
public sealed class LawAttribute(Type subject, string name) : Attribute {
    public Type Subject { get; } = subject;
    public string Name { get; } = name;
    public string? Member { get; init; }
}

// --- [MODELS] --------------------------------------------------------------------------
public readonly record struct LawRecord(Type Subject, string Name, Option<string> Member, Type DeclaringType) {
    public string CoveredSymbol => Member.IfNone(Subject.Name);
}

public readonly record struct SutTarget(Assembly Assembly, FrozenSet<string> ExemptNames);

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Laws {
    private static readonly FrozenSet<string> SynthesizedNames =
        new[] { "Equals", "GetHashCode", "ToString", "Deconstruct", "Clone", "PrintMembers", "EqualityContract", "op_Equality", "op_Inequality" }
            .ToFrozenSet(StringComparer.Ordinal);

    public static Seq<LawRecord> ScanAssembly(Assembly specAssembly) {
        ArgumentNullException.ThrowIfNull(argument: specAssembly);
        return toSeq(specAssembly.GetTypes())
            .Bind(type => Records(type.GetCustomAttributes<LawAttribute>(inherit: false), type)
                + toSeq(type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly))
                    .Bind(method => Records(method.GetCustomAttributes<LawAttribute>(inherit: false), type)));
    }

    public static SutTarget Sut(Assembly sutAssembly) {
        ArgumentNullException.ThrowIfNull(argument: sutAssembly);
        return new SutTarget(
            Assembly: sutAssembly,
            ExemptNames: toSeq(sutAssembly.GetTypes())
                .Bind(ExemptNamesOf)
                .ToFrozenSet(StringComparer.Ordinal));
    }

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

    // --- [SURFACE_CENSUS]
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

    private static bool IsExemptAttribute(object attribute) =>
        attribute switch {
            CspExemptAttribute => true,
            CspScopeAttribute scoped => scoped.Scope == CspScope.Tooling,
            _ => false,
        };
}
