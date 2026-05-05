using System.Collections.Immutable;
using Foundation.CSharp.Analyzers.Dispatch;
using Foundation.CSharp.Analyzers.Kernel;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Foundation.CSharp.Analyzers;

// --- [ANALYZER] --------------------------------------------------------------

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class DomainStandardsAnalyzer : DiagnosticAnalyzer {
    // --- [DIAGNOSTICS] --------------------------------------------------------

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => RuleCatalog.All;

    // --- [ENTRY_POINT] --------------------------------------------------------

    public override void Initialize(AnalysisContext context) {
        _ = context ?? throw new ArgumentNullException(paramName: nameof(context));
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterCompilationStartAction(startContext => {
            AnalyzerState state = AnalyzerState.Create(compilation: startContext.Compilation);
            startContext.RegisterSymbolAction(symbolContext => AnalyzerDispatcher.Run(symbolContext, state), SymbolKind.Method, SymbolKind.Property, SymbolKind.NamedType);
            startContext.RegisterOperationAction(
                operationContext => AnalyzerDispatcher.Run(operationContext, state),
                OperationKind.Invocation,
                OperationKind.PropertyReference,
                OperationKind.ObjectCreation,
                OperationKind.AnonymousFunction,
                OperationKind.Conditional,
                OperationKind.Loop,
                OperationKind.Try,
                OperationKind.Throw,
                OperationKind.Binary,
                OperationKind.IsPattern,
                OperationKind.SimpleAssignment,
                OperationKind.Literal,
                OperationKind.Await,
                OperationKind.Conversion,
                OperationKind.With);
            startContext.RegisterSyntaxNodeAction(
                syntaxContext => AnalyzerDispatcher.Run(syntaxContext, state),
                SyntaxKind.VariableDeclaration,
                SyntaxKind.ForEachStatement,
                SyntaxKind.DeclarationExpression);
            startContext.RegisterCompilationEndAction(compilationContext => AnalyzerDispatcher.Run(compilationContext, state));
        });
    }
}
