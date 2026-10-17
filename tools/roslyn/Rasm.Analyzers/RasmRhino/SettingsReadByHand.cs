using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Rasm.Analyzers.RasmRhino;

// --- [COMPOSITION] ---------------------------------------------------------------------
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class SettingsReadByHandAnalyzer : RasmAnalyzer {
    private static readonly DiagnosticDescriptor Read = Rule(
        "RASM2101",
        "Rasm.Rhino",
        "PersistentSettings read outside SettingType",
        "`{0}` reads `PersistentSettings` outside `SettingType`",
        "Read through `SettingType.<Kind>.Read(<settings>, <key>)`, which fails with `Errors.ParseError` on stored text that direct reads return as absent or default.");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Read];

    protected override void Start(CompilationStartAnalysisContext context) {
        Compilation compilation = context.Compilation;
        if (compilation.GetTypeByMetadataName("Rhino.PersistentSettings") is not { } settings
            || compilation.GetTypeByMetadataName("Rasm.Rhino.Persistence.Settings.SettingType") is not { } rows
            || compilation.GetTypeByMetadataName("Rasm.Rhino.Persistence.Settings.SettingType`1") is not { } row) return;
        ImmutableHashSet<string> names = ImmutableHashSet.CreateRange(StringComparer.Ordinal, settings.MemberNames);
        ImmutableHashSet<string> getters = ImmutableHashSet.CreateRange(StringComparer.Ordinal, names
            .Where(name => name.StartsWith("Set", StringComparison.Ordinal) && names.Contains("Get" + name.Substring(3)))
            .SelectMany(static name => new[] { "Get" + name.Substring(3), "TryGet" + name.Substring(3) }));
        ImmutableArray<IMethodSymbol> reads = [.. settings.GetMembers().OfType<IMethodSymbol>().Where(method => method.Parameters.Length > 1 && getters.Contains(method.Name))];
        context.RegisterOperationAction(
            operation => {
                IInvocationOperation invocation = (IInvocationOperation)operation.Operation;
                if (reads.Any(read => Same(invocation.TargetMethod, read)) && !Within(operation.ContainingSymbol, rows) && !Within(operation.ContainingSymbol, row))
                    operation.ReportDiagnostic(Diagnostic.Create(Read, invocation.Syntax.GetLocation(), invocation.TargetMethod.Name));
            },
            OperationKind.Invocation);
    }
}
