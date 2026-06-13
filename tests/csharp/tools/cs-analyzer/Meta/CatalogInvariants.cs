using System.Collections.Immutable;
using System.Globalization;
using System.Reflection;
using System.Xml.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis.Text;
using Rasm.Csp.Kernel;

namespace Rasm.Csp.Tests.Meta;

// --- [OPERATIONS] ----------------------------------------------------------------------

// Catalog meta-invariants (a)-(h) plus docs-sync (i). Empty catalog state is intentional:
// rule-owned checks go vacuous, but central CSP bands and docs must also stay empty.
public sealed class CatalogInvariants {
    private static readonly string[] BannedEverywhere = [
        "CultureInfo.CurrentCulture",
        "DateTime.Now",
        "DateTime.UtcNow",
        "DateTimeOffset.Now",
        "DateTimeOffset.UtcNow",
        "new Random",
        "Random.Shared",
        "Environment.GetEnvironmentVariable",
    ];

    private static readonly string[] ReservedIds = [];

    private static readonly string RepoRoot = typeof(CatalogInvariants).Assembly
        .GetCustomAttributes<AssemblyMetadataAttribute>()
        .Single(predicate: static attribute => string.Equals(attribute.Key, "RepoRoot", StringComparison.Ordinal))
        .Value!;

    // (a) Every static RuleRow declared anywhere in the analyzer assembly is registered in
    // Catalog.All, and catalog IDs are unique.
    [Fact]
    public void RowsRegistered() {
        const BindingFlags Statics = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
        List<string> unregistered = [];
        foreach (Type type in typeof(Driver).Assembly.GetTypes()) {
            foreach (FieldInfo field in type.GetFields(Statics))
                if (field.FieldType == typeof(RuleRow) && field.GetValue(obj: null) is RuleRow fromField && !Catalog.All.Contains(fromField))
                    unregistered.Add(type.FullName + "." + field.Name);
            foreach (PropertyInfo property in type.GetProperties(Statics))
                if (property.PropertyType == typeof(RuleRow) && property.GetValue(obj: null) is RuleRow fromProperty && !Catalog.All.Contains(fromProperty))
                    unregistered.Add(type.FullName + "." + property.Name);
        }
        Assert.Empty(unregistered);
        int distinct = Catalog.All.Select(selector: static row => row.Descriptor.Id).Distinct(StringComparer.Ordinal).Count();
        Assert.Equal(expected: Catalog.All.Length, actual: distinct);
    }

    // (b) Every catalog row has BOTH a positive (fires-with-span) and a negative
    // (valid-compact-code clean) fact, keyed on descriptor ID via [RuleSpec]; no orphan specs.
    [Fact]
    public void RowsTested() {
        Type[] specTypes = typeof(CatalogInvariants).Assembly.GetTypes();
        ImmutableHashSet<string> catalogIds = Catalog.All
            .Select(selector: static row => row.Descriptor.Id)
            .ToImmutableHashSet(equalityComparer: StringComparer.Ordinal);
        List<string> failures = [];
        foreach (RuleRow row in Catalog.All) {
            string id = row.Descriptor.Id;
            bool positive = false;
            bool negative = false;
            foreach (Type type in specTypes) {
                if (!SpecIds(type).Contains(value: id, comparer: StringComparer.Ordinal)) continue;
                foreach (MethodInfo method in type.GetMethods()) {
                    if (!method.IsDefined(attributeType: typeof(FactAttribute), inherit: true)) continue;
                    positive |= Applies(method.GetCustomAttribute<PositiveAttribute>()?.Ids, id);
                    negative |= Applies(method.GetCustomAttribute<NegativeAttribute>()?.Ids, id);
                }
            }
            if (!positive) failures.Add(id + ": no positive fact");
            if (!negative) failures.Add(id + ": no negative fact");
        }
        foreach (Type type in specTypes)
            foreach (string id in SpecIds(type))
                if (!catalogIds.Contains(id)) failures.Add(type.FullName + ": orphan spec for " + id);
        Assert.Empty(failures);
    }

    // (c) Every Law-tier descriptor carries the fix route in-message (fact/fix/exempt grammar).
    [Fact]
    public void LawMessagesCarryFix() {
        List<string> failures = [];
        foreach (RuleRow row in Catalog.All)
            if (row.Tier == Tier.Law && !row.Descriptor.MessageFormat.ToString(CultureInfo.InvariantCulture).Contains(value: "fix:", comparisonType: StringComparison.Ordinal))
                failures.Add(row.Descriptor.Id);
        Assert.Empty(failures);
    }

    // (d) Source hygiene: no ToDisplayString in Rules/ predicate code (type identity must use
    // SymbolEqualityComparer; display formatting lives only in the Kernel Report path), no
    // CultureInfo.CurrentCulture, and no DateTime/Random/environment reads in analyzer source.
    [Fact]
    public void SourceHygiene() {
        string analyzerRoot = Path.Combine(RepoRoot, "tools", "cs-analyzer");
        List<string> violations = [];
        foreach (string file in Directory.EnumerateFiles(analyzerRoot, searchPattern: "*.cs", searchOption: SearchOption.AllDirectories)) {
            bool excluded = file.Contains(value: "/obj/", comparisonType: StringComparison.Ordinal)
                || file.Contains(value: "/bin/", comparisonType: StringComparison.Ordinal)
                || file.Contains(value: "/Contracts/", comparisonType: StringComparison.Ordinal);
            if (excluded) continue;
            string text = CodeTokensOnly(File.ReadAllText(file));
            foreach (string token in BannedEverywhere)
                if (text.Contains(value: token, comparisonType: StringComparison.Ordinal))
                    violations.Add(file + ": " + token);
            bool predicateLeak = file.Contains(value: "/Rules/", comparisonType: StringComparison.Ordinal)
                && text.Contains(value: ".ToDisplayString(", comparisonType: StringComparison.Ordinal);
            if (predicateLeak) violations.Add(file + ": ToDisplayString in predicate code");
        }
        Assert.Empty(violations);
    }

    // Hygiene scans CODE, not prose: string-literal tokens (vocabulary DocIds legitimately name banned
    // symbols) and comment trivia are blanked via the syntax tree before the token match runs.
    private static string CodeTokensOnly(string text) {
        SyntaxNode root = Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree.ParseText(text).GetRoot();
        System.Text.StringBuilder scrubbed = new(text);
        IEnumerable<TextSpan> spans = root.DescendantTokens()
            .Where(predicate: static token => token.IsKind(Microsoft.CodeAnalysis.CSharp.SyntaxKind.StringLiteralToken)
                || token.IsKind(Microsoft.CodeAnalysis.CSharp.SyntaxKind.SingleLineRawStringLiteralToken)
                || token.IsKind(Microsoft.CodeAnalysis.CSharp.SyntaxKind.MultiLineRawStringLiteralToken)
                || token.IsKind(Microsoft.CodeAnalysis.CSharp.SyntaxKind.InterpolatedStringTextToken)
                || token.IsKind(Microsoft.CodeAnalysis.CSharp.SyntaxKind.CharacterLiteralToken))
            .Select(selector: static token => token.Span)
            .Concat(root.DescendantTrivia()
                .Where(predicate: static trivia => trivia.IsKind(Microsoft.CodeAnalysis.CSharp.SyntaxKind.SingleLineCommentTrivia)
                    || trivia.IsKind(Microsoft.CodeAnalysis.CSharp.SyntaxKind.MultiLineCommentTrivia)
                    || trivia.IsKind(Microsoft.CodeAnalysis.CSharp.SyntaxKind.SingleLineDocumentationCommentTrivia)
                    || trivia.IsKind(Microsoft.CodeAnalysis.CSharp.SyntaxKind.MultiLineDocumentationCommentTrivia))
                .Select(selector: static trivia => trivia.Span));
        foreach (TextSpan span in spans)
            for (int i = span.Start; i < span.End; i++)
                scrubbed[i] = ' ';
        return scrubbed.ToString();
    }

    // (e) Release tracking rides the REAL RS2000-RS2008 rail (AnalyzerReleases md as
    // AdditionalFiles); here: every rule has a Shipped/Unshipped row, and reserved IDs appear in
    // neither the catalog, the emitted descriptors, nor the release tracking.
    [Fact]
    public void ReleaseTracking() {
        string analyzerRoot = Path.Combine(RepoRoot, "tools", "cs-analyzer");
        string shipped = File.ReadAllText(Path.Combine(analyzerRoot, "AnalyzerReleases.Shipped.md"));
        string unshipped = File.ReadAllText(Path.Combine(analyzerRoot, "AnalyzerReleases.Unshipped.md"));
        string[] declaredReserved = [.. Catalog.Reserved.Order(StringComparer.Ordinal)];
        Assert.Equal(expected: ReservedIds, actual: declaredReserved);
        ImmutableArray<DiagnosticDescriptor> emitted = new Driver().SupportedDiagnostics;
        List<string> failures = [];
        foreach (RuleRow row in Catalog.All) {
            string id = row.Descriptor.Id;
            bool tracked = shipped.Contains(value: id, comparisonType: StringComparison.Ordinal)
                || unshipped.Contains(value: id, comparisonType: StringComparison.Ordinal);
            if (!tracked) failures.Add(id + ": no release-tracking row");
        }
        foreach (string reserved in ReservedIds) {
            if (emitted.Any(predicate: descriptor => string.Equals(descriptor.Id, reserved, StringComparison.Ordinal)))
                failures.Add(reserved + ": reserved id emitted");
            bool leaked = shipped.Contains(value: reserved, comparisonType: StringComparison.Ordinal)
                || unshipped.Contains(value: reserved, comparisonType: StringComparison.Ordinal);
            if (leaked) failures.Add(reserved + ": reserved id in release tracking");
        }
        Assert.Empty(failures);
    }

    // (f) The Rule x Scope applicability matrix is complete: every row declares a real ScopeGate.
    [Fact]
    public void ScopeGatesDeclared() {
        List<string> failures = [];
        foreach (RuleRow row in Catalog.All) {
            int gateBits = (int)row.Scope;
            if (gateBits == 0 || (gateBits & ~(int)ScopeGate.Everywhere) != 0) failures.Add(row.Descriptor.Id);
        }
        Assert.Empty(failures);
    }

    // (g) The Directory.Build.props <WarningsNotAsErrors> CSP band EXACTLY equals the catalog's
    // Pressure-tier IDs, both directions. Empty catalog state requires an empty central CSP band.
    [Fact]
    public void PressureBandParity() {
        XDocument props = XDocument.Load(Path.Combine(RepoRoot, "Directory.Build.props"));
        string joined = string.Join(separator: ';', values: props.Descendants("WarningsNotAsErrors").Select(selector: static element => element.Value));
        string[] band = [.. joined
            .Split(separator: ';', options: StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(predicate: static token => token.StartsWith(value: "CSP", comparisonType: StringComparison.Ordinal))
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)];
        string[] pressure = [.. Catalog.All
            .Where(predicate: static row => row.Tier == Tier.Pressure)
            .Select(selector: static row => row.Descriptor.Id)
            .Order(StringComparer.Ordinal)];
        Assert.Equal(expected: band, actual: pressure);
    }

    // (h) The Harness AddPackages identities EXACTLY equal the Directory.Packages.props pins:
    // a central bump without the harness bump fails here instead of silently testing rules
    // against a stale package surface.
    [Fact]
    public void HarnessPinMirror() {
        XDocument packages = XDocument.Load(Path.Combine(RepoRoot, "Directory.Packages.props"));
        foreach (PackageIdentity mirror in Harness.MirroredPackages) {
            string? pinned = packages.Descendants("PackageVersion")
                .Where(predicate: element => string.Equals((string?)element.Attribute("Include"), mirror.Id, StringComparison.OrdinalIgnoreCase))
                .Select(selector: static element => (string?)element.Attribute("Version"))
                .SingleOrDefault();
            Assert.Equal(expected: pinned, actual: mirror.Version);
        }
    }

    // (i) Docs sync: every catalog rule ID has tools/cs-analyzer/docs/rules/<ID>.md and every
    // file there has a catalog row — 1:1 in both directions, vacuous at zero rules.
    [Fact]
    public void DocsSync() {
        string docsDir = Path.Combine(RepoRoot, "tools", "cs-analyzer", "docs", "rules");
        string[] docIds = Directory.Exists(docsDir)
            ? [.. Directory.EnumerateFiles(docsDir, searchPattern: "*.md")
                .Select(selector: static file => Path.GetFileNameWithoutExtension(file))
                .Order(StringComparer.Ordinal)]
            : [];
        string[] catalogIds = [.. Catalog.All
            .Select(selector: static row => row.Descriptor.Id)
            .Order(StringComparer.Ordinal)];
        Assert.Equal(expected: catalogIds, actual: docIds);
    }

    private static bool Applies(IReadOnlyList<string>? ids, string id) =>
        ids is not null && (ids.Count == 0 || ids.Contains(value: id, comparer: StringComparer.Ordinal));

    private static IEnumerable<string> SpecIds(Type type) =>
        type.GetCustomAttributes<RuleSpecAttribute>(inherit: false).Select(selector: static spec => spec.Id);
}
