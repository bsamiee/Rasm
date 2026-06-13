using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace Rasm.Csp.Kernel;

// --- [TYPES] ---------------------------------------------------------------------------

// Analyzer-side mirror of Rasm.Csp.CspScope in Csp.Contracts: the analyzer matches the contract
// attributes by metadata name only — RS1038 forbids referencing the contracts assembly here.
internal enum CspScope { Domain, Application, Boundary, Composition, HotPath, Test, Tooling }

internal enum Tier : byte { Law, Pressure, Info }              // Law=Error, Pressure=Warning, Info=Info

internal enum Category : byte { Flow, Shape, Surface, Rail, Boundary, Perf }

[Flags]
internal enum ScopeGate : byte {
    Domain = 1, Application = 2, Boundary = 4, Composition = 8, HotPath = 16, Test = 32, Tooling = 64,
    // HotPath is a domain SPECIALIZATION: it adds the perf band (0601), never exits domain discipline.
    DomainOrApplication = Domain | Application | HotPath,
    Everywhere = Domain | Application | Boundary | Composition | HotPath | Test | Tooling,
}

// --- [TABLES] --------------------------------------------------------------------------

internal static class Catalog {
    // Per-rule docs live beside the analyzer only when a real catalog row exists; docs sync is
    // meta-tested so the empty catalog carries no orphan rule prose.
    private const string HelpBase = "tools/cs-analyzer/docs/rules/";

    // The ONLY registry; the meta-test reflects assembly RuleRow statics into it.
    public static readonly ImmutableArray<RuleRow> All = [];

    // Never emitted, never reused.
    public static readonly ImmutableArray<string> Reserved = [];

    // Description = doctrine page#section; messageFormat carries the fact/fix/exempt grammar;
    // helpLinkUri points at the rule's own doc file.
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
