using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace Rasm.Csp.Kernel;

// --- [TYPES] ---------------------------------------------------------------------------

internal enum CspScope { Domain, Application, Boundary, Composition, HotPath, Test, Tooling }

internal enum Tier : byte { Law, Pressure, Info }

internal enum Category : byte { Flow, Shape, Surface, Rail, Boundary, Perf }

[Flags]
internal enum ScopeGate : byte {
    Domain = 1, Application = 2, Boundary = 4, Composition = 8, HotPath = 16, Test = 32, Tooling = 64,
    DomainOrApplication = Domain | Application | HotPath,
    Everywhere = Domain | Application | Boundary | Composition | HotPath | Test | Tooling,
}

// --- [TABLES] --------------------------------------------------------------------------

internal static class Catalog {
    private const string HelpBase = "tools/cs-analyzer/docs/rules/";

    public static readonly ImmutableArray<RuleRow> All = [];

    public static readonly ImmutableArray<string> Reserved = [];

    internal static DiagnosticDescriptor Describe(
        string id,
        string title,
        string messageFormat,
        Category category,
        Tier tier,
        string doctrineAnchor) =>
        new(
            id: id,
            title: title,
            messageFormat: messageFormat,
            category: category.ToString(),
            defaultSeverity: tier switch {
                Tier.Law => DiagnosticSeverity.Error,
                Tier.Pressure => DiagnosticSeverity.Warning,
                _ => DiagnosticSeverity.Info,
            },
            isEnabledByDefault: true,
            description: doctrineAnchor,
            helpLinkUri: HelpBase + id + ".md");
}
