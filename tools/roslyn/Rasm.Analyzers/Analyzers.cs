using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Rasm.Analyzers;

// --- [COMPOSITION] ---------------------------------------------------------------------
public abstract class RasmAnalyzer : DiagnosticAnalyzer {
    public sealed override void Initialize(AnalysisContext context) {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.RegisterCompilationStartAction(Start);
    }

    protected abstract void Start(CompilationStartAnalysisContext context);

    protected static DiagnosticDescriptor Rule(string id, string category, string title, string message, string description) =>
        new(id, title, message, category, DiagnosticSeverity.Error, isEnabledByDefault: true, description);

    protected static bool Same(ISymbol? symbol, ISymbol? other) =>
        SymbolEqualityComparer.Default.Equals(symbol?.OriginalDefinition, other?.OriginalDefinition);

    protected static bool Within(ISymbol? symbol, INamedTypeSymbol owner) =>
        symbol is not null && (Same(symbol, owner) || Within(symbol.ContainingSymbol, owner));

    protected static bool Derives(ITypeSymbol? type, INamedTypeSymbol root) =>
        type is not null && (Same(type, root) || Derives(type.BaseType, root));

    protected static bool InNameOf(IOperation operation) =>
        operation.Parent is { } parent && (parent is INameOfOperation || InNameOf(parent));

    protected static IOperation? Consumer(IOperation operation) =>
        operation.Parent is IConversionOperation { IsImplicit: true } conversion ? Consumer(conversion) : operation.Parent;
}
