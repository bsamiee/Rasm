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
    // Ecosystem-standard per-rule docs (Meziantou/xunit.analyzers shape): one file per rule under the
    // analyzer, catalog<->docs sync meta-tested. Monolithic prose catalogs are rejected (user decision
    // 2026-06-12); doctrine purpose law lives in docs/stacks/csharp/README.md [RULE_ENFORCEMENT].
    private const string HelpBase = "tools/cs-analyzer/docs/rules/";

    // The ONLY registry; the meta-test reflects assembly RuleRow statics into it.
    public static readonly ImmutableArray<RuleRow> All = [];

    // Never emitted, never reused.
    public static readonly ImmutableArray<string> Reserved = ["CSP0016", "CSP0716", "CSP0721", "CSP0722"];

    // Replaces the old Error-only `Err` factory: Description = doctrine page#section;
    // messageFormat in the fact/fix/exempt grammar; helpLinkUri = the rule's own doc file.
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
